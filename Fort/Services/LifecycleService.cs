using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fort.Services
{
  public class LifecycleService
  {
    public LifecycleService(IServiceScopeFactory serviceScopeFactory, ConnectionsService connectionsService)
    {
      _serviceScopeFactory = serviceScopeFactory;
      _connectionsService = connectionsService;
    }

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConnectionsService _connectionsService;

    private Turn _currentTurn;
    private TimeSpan? _remainingTurnDuration;
    private Task _lifecycleTask;
    private CancellationTokenSource _cancel;
    private DateTime _now => DateTime.UtcNow;
    private object _actionLock = new object();
    private object _finalizeLock = new object();
    private bool _finalizeLockTaken = false;

    public ELifecycleState State
    {
      get
      {
        if (_currentTurn.StartsAt == null && _currentTurn.EndsAt == null)
          return ELifecycleState.Ready;

        if (_currentTurn.StartsAt == null)
          return ELifecycleState.End;

        if (_currentTurn.EndsAt == null)
          return ELifecycleState.Paused;

        if (_currentTurn.EndsAt > DateTime.UtcNow)
          return ELifecycleState.Running;

        return ELifecycleState.Finalizing;
      }
    }
    public DateTime? TurnEndsAt
    {
      get
      {
        switch (State)
        {
          case ELifecycleState.Running:
          case ELifecycleState.Finalizing:
            return _currentTurn.EndsAt;

          // don't wait
          default:
            return null;
        }
      }
    }
    public int CurrentTurnId => _currentTurn?.Id ?? -1;

    public void Init(FortDbContext db)
    {
      _currentTurn = _getDbTurn(db);

      // init
      if (_currentTurn == null)
      {
        _initGame(db);
      }

      // ready || end
      else if (_currentTurn.StartsAt == null)
      {
        // do nothing, wait
      }

      // running || finalising || paused
      else
      {
        _remainingTurnDuration = ConfigManager.GetDuration(_now);
        _currentTurn.EndsAt = null;
      }

      db.SaveChanges();
    }

    private void RunLifecycle()
    {
      _lifecycleTask = Lifecycle();
    }
    private async Task Lifecycle()
    {
      while (State == ELifecycleState.Running || State == ELifecycleState.Finalizing)
      {
        // run
        switch (State)
        {
          case ELifecycleState.Running:
            await Wait(TurnEndsAt);
            break;
          case ELifecycleState.Finalizing:
            FinalizeTurn();
            await Wait(TimeSpan.FromSeconds(ConfigManager.Game.Animations.PauseBeforeArmyRunSec));
            break;
        }

        // send to all connected users
        var _ = SendToAllStateChanged();
      }
    }

    #region Actions
    public void StartGame(FortDbContext db)
    {
      lock (_actionLock)
      {
        if (State != ELifecycleState.Ready)
          throw new Exception($"Cannot start turn while in '{State}' state");

        _currentTurn = _getDbTurn(db);
        _currentTurn.StartsAt = _now;
        _currentTurn.EndsAt = ConfigManager.GetTurnEnd(_now);
        db.SaveChanges();

        RunLifecycle();
        
        SendToAllStateChanged();
      }
    }
    public void PauseTurn(FortDbContext db)
    {
      lock (_actionLock)
      {
        if (State != ELifecycleState.Running)
          throw new Exception($"Cannot pause turn while in '{State}' state");

        // set paused
        _currentTurn = _getDbTurn(db);
        _remainingTurnDuration = _currentTurn.EndsAt.Value - _now;
        _currentTurn.EndsAt = null;
        db.SaveChanges();

        // end running
        _cancel.Cancel();

        SendToAllStateChanged();
      }
    }
    public void ResumeTurn(FortDbContext db)
    {
      lock (_actionLock)
      {
        if (State != ELifecycleState.Paused)
          throw new Exception($"Cannot resume turn while in '{State}' state");

        _currentTurn = _getDbTurn(db);
        _currentTurn.EndsAt = (_now + _remainingTurnDuration) ?? ConfigManager.GetTurnEnd(_now);
        db.SaveChanges();

        RunLifecycle();

        SendToAllStateChanged();
      }
    }
    public void EndTurn(FortDbContext db)
    {
      lock (_actionLock)
      {
        if (State != ELifecycleState.Running)
          throw new Exception($"Cannot end turn while in '{State}' state");

        _currentTurn = _getDbTurn(db);
        _currentTurn.EndsAt = _now;
        db.SaveChanges();

        _cancel.Cancel();

        SendToAllStateChanged();
      }
    }
    public void ResetGame(FortDbContext db)
    {
      lock (_actionLock)
      {
        db.Database.ExecuteSqlCommand("DELETE FROM Turns;");
        _currentTurn = null;

        Init(db);

        // foreach (var userConnection in _connectionsService.GetAllConnections())
        //   userConnection.Send(Guid.NewGuid(), "player/resetGame", )
      }
    }
    private void FinalizeTurn()
    {
      // finalize is running
      if (_finalizeLockTaken)
        return;

      Task.Run(() =>
      {
        try
        {
          Monitor.Enter(_finalizeLock, ref _finalizeLockTaken);

          using (var scope = _serviceScopeFactory.CreateScope())
          {
            var db = scope.ServiceProvider.GetService<FortDbContext>();
            var nextTurn = _createNextTurn(db);
            db.Add(nextTurn);

            // game ends?
            if (nextTurn.CityOccupations
              .Where(co => co.Owner != null)
              .GroupBy(co => co.Owner?.TeamId)
              .Count() < 2)
            {
              nextTurn.StartsAt = null;
              nextTurn.EndsAt = _currentTurn.EndsAt;
            }
            else
            {
              nextTurn.StartsAt = _currentTurn.EndsAt;
              nextTurn.EndsAt = ConfigManager.GetTurnEnd(_currentTurn.EndsAt.Value);
            }
            db.SaveChanges();

            // current turn
            _currentTurn = nextTurn;

            // send result
            var sendingTasks = _connectionsService.GetAllUserConnections()
              .Select(i => i.connection.Send(Guid.NewGuid(), "player/turnFinalized", new { turn = new TurnManager(i.user, db, this).GetTurn(_currentTurn.Id) }))
              .ToArray();
          }
        }
        finally
        {
          if (_finalizeLockTaken)
          {
            Monitor.Exit(_finalizeLock);
            _finalizeLockTaken = false;
          }
        }
      });
    }
    #endregion

    #region Helpers
    private Turn _getDbTurn(FortDbContext db)
    {
      return db.Turns
          .OrderByDescending(t => t.Id)
          .FirstOrDefault();
    }
    private Task Wait(TimeSpan waitDuration)
    {
      _cancel = new CancellationTokenSource();
      return Task.Delay(waitDuration, _cancel.Token);
    }
    private Task Wait(DateTime? waitTill)
    {
      if (waitTill == null)
        return Task.CompletedTask;

      var waitDuration = waitTill.Value - _now;
      if (waitDuration < TimeSpan.Zero)
        return Task.CompletedTask;

      return Wait(waitDuration);
    }
    private Task SendToAllStateChanged()
    {
      // send to all connected users
      var tasks = _connectionsService.GetAllConnections()
        .Select(c => c?.Send(Guid.NewGuid(), "player/stateChanged", new { state = new Models.Store.TurnState { EndsAt = _currentTurn.EndsAt, Key = State.ToString() }}))
        .ToArray();

      return Task.WhenAll(tasks);
    }
    private void _initGame(FortDbContext db)
    {
      // create turn
      _currentTurn = new Turn
      {
        Id = 0
      };
      db.Add(_currentTurn);

      // set occupations
      var random = new Random();
      var startingPositions = db.StartingPositions.ToDictionary(p => p.CityId, p => p);

      foreach (var city in db.Cities)
      {
        int army;
        Guid? ownerId = null;
        // city owned by player
        if (startingPositions.TryGetValue(city.Id, out var position))
        {
          army = position.Army ?? ConfigManager.Game.Population.DefaultPlayerStartPopulation;
          ownerId = position.UserId;
        }
        // generate neutral
        else
        {
          army = random.Next(ConfigManager.Game.Population.NeutralCitiesPopulationMin, ConfigManager.Game.Population.NeutralCitiesPopulationMax);
        }

        var occupation = new CityOccupation
        {
          Turn = _currentTurn,
          CityId = city.Id,
          OwnerId = ownerId,
          Army = army
        };
        db.CityOccupations.Add(occupation);
      }
    }
    private Turn _createNextTurn(FortDbContext db)
    {
      // create new turn
      var nextTurn = new Turn
      {
        Id = CurrentTurnId + 1
      };

      // get results
      foreach (var cityOccupation in db.CityOccupations
        .Include(c => c.Owner)
        .Where(c => c.TurnId == CurrentTurnId))
      {
        // get armies
        var armyOut = db.Orders
          .Where(o => o.TurnId == CurrentTurnId && (o.StIsSource ? o.StCityId : o.NdCityId) == cityOccupation.CityId)
          .Sum(o => o.Amount);
        var armyIn = db.Orders
          .Where(o => o.TurnId == CurrentTurnId && (o.StIsSource ? o.NdCityId : o.StCityId) == cityOccupation.CityId && cityOccupation.Owner != null && o.User.TeamId == cityOccupation.Owner.TeamId)
          .Sum(o => o.Amount);
        var armyCounter = db.Orders
          .Where(o => o.TurnId == CurrentTurnId && (o.StIsSource ? o.NdCityId : o.StCityId) == cityOccupation.CityId && (cityOccupation.Owner == null || o.User.TeamId != cityOccupation.Owner.TeamId))
          .Sum(o => o.Amount);

        // new occupation
        var result = cityOccupation.Army - armyOut + armyIn - armyCounter;
        User owner;
        // conquered
        if (result < 0)
        {
          // biggest enemy army
          owner = db.Orders
            .Include(o => o.User)
            .Where(o => o.TurnId == CurrentTurnId && (o.StIsSource ? o.NdCityId : o.StCityId) == cityOccupation.CityId && (cityOccupation.Owner == null || o.User.TeamId != cityOccupation.Owner.TeamId))
            .OrderByDescending(o => o.Amount)
            .First().User;
          result = -result;
        }
        // defended
        else
        {
          owner = cityOccupation.Owner;
        }

        // population grow only for users
        var populationGrow = owner != null
          ? ConfigManager.Game.Population.DefaultTurnGrow
          : 0;

        // create
        var newOccupation = new CityOccupation
        {
          CityId = cityOccupation.CityId,
          Owner = owner,
          Army = result + populationGrow
        };
        nextTurn.CityOccupations.Add(newOccupation);
      }

      return nextTurn;
    }
    #endregion
  }
}
