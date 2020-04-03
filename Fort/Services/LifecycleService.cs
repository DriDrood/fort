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
        public LifecycleService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _remaining = ConfigManager.GetDuration();
        }

        private readonly IServiceScopeFactory _serviceScopeFactory;

        private Turn _currentTurn;
        private TimeSpan? _remaining;
        private Task _lifecycleTask;
        private CancellationTokenSource _cancel;

        public ELifecycleState State
        {
            get
            {
                if (_currentTurn == null)
                    return ELifecycleState.Init;

                if (_currentTurn.StartsAt == null)
                    return ELifecycleState.Stopped;

                if (_currentTurn.EndsAt == null)
                    return ELifecycleState.Paused;

                if (_currentTurn.EndsAt < DateTime.UtcNow)
                    return ELifecycleState.Finalizing;

                return ELifecycleState.Running;
            }
        }
        public int CurrentTurnId => _currentTurn?.Id ?? -1;

        public void Setup()
        {
            // get current turn & state
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<FortDbContext>();
                _currentTurn = db.Turns
                    .OrderByDescending(t => t.Id)
                    .FirstOrDefault();
            }

            // continue
            RunLifecycle();
        }

        public void RunLifecycle()
        {
            _cancel = new CancellationTokenSource();
            _lifecycleTask = Task.Run(Lifecycle);
        }

        private async Task Lifecycle()
        {
            while (!(IsGameEnd() || _cancel.IsCancellationRequested))
            {
                // init
                TimeSpan wait = TimeSpan.Zero;

                // do
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetService<FortDbContext>();

                    switch (State)
                    {
                        case ELifecycleState.Init:
                            _startGame(db);
                            break;
                        case ELifecycleState.Stopped:
                            _cancel.Cancel();
                            break;
                        case ELifecycleState.Running:
                            wait = _currentTurn.EndsAt.Value - DateTime.UtcNow;
                            break;
                        case ELifecycleState.Paused:
                            _cancel.Cancel();
                            break;
                        case ELifecycleState.Finalizing:
                            _endTurn(db);
                            wait = TimeSpan.FromSeconds(ConfigManager.Game.Animations.PauseBeforeArmyRunSec);
                            break;
                    }

                    db.SaveChanges();
                }

                // wait
                try
                {
                    await Task.Delay(wait, _cancel.Token);
                }
                catch(TaskCanceledException)
                { }
            }
        }

        private bool IsGameEnd()
        {
            return false;
            throw new NotImplementedException();
        }

        #region public status change
        public void ResetGame(FortDbContext db)
        {
            _cancel.Cancel();
            db.Database.ExecuteSqlCommand("DELETE FROM Turns;");
            _currentTurn = null;
            
            RunLifecycle();
        }
        public void StartTurn(FortDbContext db)
        {
            if (State != ELifecycleState.Stopped && State != ELifecycleState.Finalizing)
                throw new Exception($"Cannot start turn while in '{State}' state");

            _cancel.Cancel();
            _startTurn(db);
            RunLifecycle();
        }
        public void PauseTurn(FortDbContext db)
        {
            if (State != ELifecycleState.Running)
                throw new Exception($"Cannot pause turn while in '{State}' state");

            _cancel.Cancel();
            _pauseTurn(db);
        }
        public void ResumeTurn(FortDbContext db)
        {
            if (State != ELifecycleState.Paused)
                throw new Exception($"Cannot resume turn while in '{State}' state");

            _cancel.Cancel();
            _resumeTurn(db);
            RunLifecycle();
        }
        public void EndTurn(FortDbContext db)
        {
            if (State != ELifecycleState.Running)
                throw new Exception($"Cannot end turn while in '{State}' state");

            _cancel.Cancel();
            _endTurn(db);
            RunLifecycle();
        }
        #endregion

        #region private status change
        private void _startGame(FortDbContext db)
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
                    Army = army + (ownerId != null ? ConfigManager.Game.Population.DefaultTurnGrow : 0)
                };
                db.CityOccupations.Add(occupation);
            }
        }
        private void _startTurn(FortDbContext db)
        {
            _currentTurn = db.Turns.Find(CurrentTurnId);

            var now = DateTime.UtcNow;
            _currentTurn.StartsAt = now;
            _currentTurn.EndsAt = ConfigManager.GetTurnEnd(now);
        }
        private void _pauseTurn(FortDbContext db)
        {
            _currentTurn = db.Turns.Find(CurrentTurnId);
            var now = DateTime.UtcNow;

            _remaining = _currentTurn.EndsAt - now;
            _currentTurn.EndsAt = null;
        }
        private void _resumeTurn(FortDbContext db)
        {
            _currentTurn = db.Turns.Find(CurrentTurnId);
            var now = DateTime.UtcNow;

            _currentTurn.EndsAt = now + _remaining;
        }
        private void _endTurn(FortDbContext db)
        {
            // create new turn
            var turn = new Turn
            {
                Id = CurrentTurnId + 1
            };
            db.Add(turn);

            // get results
            foreach (var cityOccupation in db.CityOccupations
                .Include(c => c.Owner)
                .Where(c => c.TurnId == CurrentTurnId))
            {
                // get armies
                var armyOut = db.Orders
                    .Where(o => o.TurnId == CurrentTurnId && o.SourceCityId == cityOccupation.CityId)
                    .Sum(o => o.Amount);
                var armyIn = db.Orders
                    .Where(o => o.TurnId == CurrentTurnId && o.TargetCityId == cityOccupation.CityId && cityOccupation.Owner != null && o.User.TeamId == cityOccupation.Owner.TeamId)
                    .Sum(o => o.Amount);
                var armyCounter = db.Orders
                    .Where(o => o.TurnId == CurrentTurnId && o.TargetCityId == cityOccupation.CityId && (cityOccupation.Owner == null || o.User.TeamId != cityOccupation.Owner.TeamId))
                    .Sum(o => o.Amount);

                // new occupation
                var result = cityOccupation.Army - armyOut + armyIn - armyCounter;
                Guid? ownerId;
                // conquered
                if (result < 0)
                {
                    // biggest enemy army
                    ownerId = db.Orders
                        .Where(o => o.TurnId == CurrentTurnId && o.TargetCityId == cityOccupation.CityId && o.User.TeamId != cityOccupation.Owner.TeamId)
                        .OrderByDescending(o => o.Amount)
                        .First().UserId;
                    result = -result;
                }
                // defended
                else
                {
                    ownerId = cityOccupation.OwnerId;
                }

                var newOccupation = new CityOccupation
                {
                    Turn = turn,
                    CityId = cityOccupation.CityId,
                    OwnerId = ownerId,
                    Army = result
                };
                db.Add(newOccupation);
            }

            _currentTurn = turn;

            _startTurn(db);
        }
        #endregion
    }
}