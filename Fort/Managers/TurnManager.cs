using System;
using System.Linq;
using Fort.Database;
using Fort.Models.Params;
using Fort.Models.Store;
using Fort.Services;
using Microsoft.EntityFrameworkCore;

namespace Fort.Managers
{
    public class TurnManager
    {
        public TurnManager(FortDbContext db, LifecycleService lifecycleService)
        {
            _db = db;
            _lifecycleService = lifecycleService;
        }

        private readonly FortDbContext _db;
        private readonly LifecycleService _lifecycleService;

        public CurrentTurn GetCurrentTurn()
        {
            var currentTurn = _db.Turns.Find(_lifecycleService.CurrentTurnId);
            if (currentTurn == null)
                return null;

            var turn = GetTurn(currentTurn.Id);
            return new CurrentTurn
            {
                Id = currentTurn.Id,
                State = _lifecycleService.State.ToString(),
                EndsAt = _lifecycleService.State == ELifecycleState.Finalizing
                    ? currentTurn.EndsAt.Value.AddSeconds(ConfigManager.Game.Animations.PauseBeforeArmyRunSec)
                    : currentTurn.EndsAt,
                Turn = turn
            };
        }

        public Turn GetTurn(int id)
        {
            var turnDb = _db.Turns
                .Include(t => t.CityOccupations)
                .Include(t => t.Orders)
                .SingleOrDefault(t => t.Id == id);

            var cityOccupations = turnDb.CityOccupations
                .ToDictionary(
                    c => c.CityId,
                    c => new CityOccupation
                    {
                        PlayerId = c.OwnerId,
                        Size = GetCitySize(c.Army),
                        Army = IsFriendly(c.CityId) ? (int?)c.Army : null,
                        AvailableArmy = IsFriendly(c.CityId) ? (int?)(c.Army - turnDb.Orders.Where(o => o.SourceCityId == c.CityId).Sum(o => o.Amount)) : null
                    });
            var orders = turnDb.Orders
                .ToDictionary(
                    o => $"{o.SourceCityId}>>{o.TargetCityId}",
                    o => new Order
                    {
                        PlayerId = o.UserId,
                        Size = GetOrderSize(o.Amount),
                        Amount = IsFriendly(o.SourceCityId) ? (int?)o.Amount : null
                    });

            var turn = new Turn
            {
                CityOccupations = cityOccupations,
                Orders = orders
            };
            return turn;
        }

        public void SetOrder(OrderParams order, Guid playerId, int turnId)
        {
            var dbOrder = _db.Orders.SingleOrDefault(o => o.TurnId == turnId && o.SourceCityId == order.SourceId && o.TargetCityId == order.TargetId);

            // add
            if (dbOrder == null)
            {
                dbOrder = new Database.Entities.Order
                {
                    TurnId = turnId,
                    Amount = order.Amount,
                    SourceCityId = order.SourceId,
                    TargetCityId = order.TargetId,
                    UserId = playerId
                };
                _db.Orders.Add(dbOrder);
            }

            // remove
            else if (order.Amount == 0)
            {
                _db.Orders.Remove(dbOrder);
            }

            // update
            else
            {
                dbOrder.Amount = order.Amount;
            }

            _db.SaveChanges();
        }

        private bool IsFriendly(Guid city)
        {
            return true;
            throw new NotImplementedException();
        }

        private int GetCitySize(int army)
        {
            return (int)Math.Sqrt(army) + 10;
        }
        private int GetOrderSize(int army)
        {
            return (int)Math.Sqrt(army) + 5;
        }
    }
}