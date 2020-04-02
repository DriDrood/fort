using System;
using System.Linq;
using Fort.Database;
using Fort.Models;
using Fort.Models.Store;
using Microsoft.EntityFrameworkCore;

namespace Fort.Managers
{
    public class TurnManager
    {
        public TurnManager(FortDbContext db)
        {
            _db = db;
        }

        private readonly FortDbContext _db;

        public CurrentTurn GetCurrentTurn()
        {
            var lastTurn = _db.Turns
                .OrderByDescending(t => t.Id)
                .FirstOrDefault();
            if (lastTurn == null)
                return null;

            var turn = GetTurn(lastTurn.Id);
            return new CurrentTurn
            {
                Id = lastTurn.Id,
                EndsAt = lastTurn.EndsAt,
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
                        Army = IsFriendly(c.CityId) ? (int?)c.Army : null
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

        public void SetOrder(OrderData order, Guid playerId, int turnId)
        {
            var dbOrder = new Database.Entities.Order
            {
                TurnId = turnId,
                Amount = order.Amount,
                SourceCityId = order.SourceId,
                TargetCityId = order.TargetId,
                UserId = playerId
            };
            _db.Orders.Add(dbOrder);
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