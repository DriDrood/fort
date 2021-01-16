using System;
using System.Collections.Generic;
using System.Linq;
using Fort.Database;
using Fort.Models;
using Fort.Models.Params;
using Fort.Models.Store;
using Fort.Services;
using Microsoft.EntityFrameworkCore;

namespace Fort.Managers
{
    public class TurnManager
    {
        public const int DEFAULT_CITY_SIZE = 10;

        public TurnManager(JwtUser jwtUser, FortDbContext db, LifecycleService lifecycleService)
        {
            _jwtUser = jwtUser;
            _db = db;
            _lifecycleService = lifecycleService;
        }

        private readonly JwtUser _jwtUser;
        private readonly FortDbContext _db;
        private readonly LifecycleService _lifecycleService;

        private readonly Dictionary<int, HashSet<Guid>> _visibleCities = new Dictionary<int, HashSet<Guid>>();
        private readonly Dictionary<int, HashSet<Guid>> _friendlyCities = new Dictionary<int, HashSet<Guid>>();

        public TurnState GetTurnState()
        {
            var currentTurn = _db.Turns.Find(_lifecycleService.CurrentTurnId);
            if (currentTurn == null)
                return null;

            return new TurnState
            {
                Id = currentTurn.Id,
                Key = _lifecycleService.State.ToString(),
                EndsAt = _lifecycleService.WaitTill,
            };
        }

        public Turn GetTurn(int id)
        {
            var cityOccupations = _db.CityOccupations
                .Where(co => co.TurnId == id)
                .ToDictionary(
                    c => c.CityId,
                    c => new CityOccupation
                    {
                        PlayerId = c.OwnerId,
                        Size = IsVisible(c) ? GetCitySize(c.Army) : DEFAULT_CITY_SIZE,
                        Army = IsFriendly(c) ? (int?)c.Army : null,
                        AvailableArmy = IsMy(c) ? (int?)(c.Army - _db.Orders.Where(o => o.TurnId == id && (o.StIsSource ? o.StCityId : o.NdCityId) == c.CityId).Sum(o => o.Amount)) : null
                    });

            var visibleCities = GetVisibleCities(id);
            var orders = _db.Orders
                .Include(o => o.Road).ThenInclude(r => r.Orders)
                // filter by turn
                .Where(o => o.TurnId == id)
                // filter visible
                .Where(o => visibleCities.Contains(o.StCityId) || visibleCities.Contains(o.NdCityId))
                .ToDictionary(
                    o => o.StIsSource ? $"{o.StCityId}>>{o.NdCityId}" : $"{o.NdCityId}>>{o.StCityId}",
                    o => new Order
                    {
                        Id = o.StIsSource ? $"{o.StCityId}>>{o.NdCityId}" : $"{o.NdCityId}>>{o.StCityId}",
                        PlayerId = o.UserId,
                        StartSize = GetOrderSize(o.Amount),
                        EndSize = GetOrderSize(o.Amount - o.Road.Orders.Where(ro => ro.StIsSource != o.StIsSource).Sum(ro => ro.Amount)),
                        StartAmount = IsMy(o) ? (int?)o.Amount : null,
                        EndAmount = IsMy(o) ? (int?)o.Amount - o.Road.Orders.Where(ro => ro.StIsSource != o.StIsSource).Sum(ro => ro.Amount) : null,
                    });

            var turn = new Turn
            {
                Id = id,
                CityOccupations = cityOccupations,
                Orders = orders
            };
            return turn;
        }

        public Order SetOrder(OrderParams order, Guid playerId, int turnId)
        {
            if (_lifecycleService.State != ELifecycleState.Running && _lifecycleService.State != ELifecycleState.Paused)
                throw new Exception("Turn is not running");

            var stIsSource = string.Compare(order.SourceId.ToString(), order.TargetId.ToString()) < 0;
            var dbOrder = _db.Orders.SingleOrDefault(o =>
                o.TurnId == turnId
                && o.StCityId == (stIsSource ? order.SourceId : order.TargetId)
                && o.NdCityId == (stIsSource ? order.TargetId : order.SourceId)
                && o.StIsSource == stIsSource);

            // validate
            var sourceCity = _db.CityOccupations
                .Include(co => co.StForOrders)
                .Include(co => co.NdForOrders)
                .SingleOrDefault(co => co.CityId == order.SourceId && co.TurnId == turnId)
                ?? throw new Exception("City not found");
            if (sourceCity.Army < ((stIsSource ? sourceCity.StForOrders : sourceCity.NdForOrders).Sum(o => o.Amount) - (dbOrder?.Amount ?? 0)))
                throw new Exception("City has not enought army");
            if (sourceCity.OwnerId != playerId)
                throw new Exception("City is not yours!");
            // deleted twice
            if (dbOrder == null && order.Amount == 0)
                return new Order
                {
                    Id = $"{order.SourceId}>>{order.TargetId}",
                    PlayerId = playerId,
                    StartSize = 0,
                    EndSize = 0,
                    StartAmount = 0,
                    EndAmount = 0,
                };

            // add
            if (dbOrder == null)
            {
                dbOrder = new Database.Entities.Order
                {
                    TurnId = turnId,
                    StIsSource = stIsSource,
                    StCityId = stIsSource ? order.SourceId : order.TargetId,
                    NdCityId = stIsSource ? order.TargetId : order.SourceId,
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
            dbOrder.Amount = order.Amount;
            _db.SaveChanges();

            var result = new Order
            {
                Id = dbOrder.StIsSource ? $"{dbOrder.StCityId}>>{dbOrder.NdCityId}" : $"{dbOrder.NdCityId}>>{dbOrder.StCityId}",
                PlayerId = dbOrder.UserId,
                StartSize = GetOrderSize(dbOrder.Amount),
                EndSize = GetOrderSize(dbOrder.Amount),
                StartAmount = IsMy(dbOrder) ? (int?)dbOrder.Amount : null,
                EndAmount = IsMy(dbOrder) ? (int?)dbOrder.Amount : null,
            };
            return result;
        }

        private bool IsVisible(Database.Entities.CityOccupation cityOccupation)
        {
            return
                GetVisibleCities(cityOccupation.TurnId)
                    .Contains(cityOccupation.CityId);
        }
        private bool IsFriendly(Database.Entities.CityOccupation cityOccupation)
        {
            return
                GetFriendlyCities(cityOccupation.TurnId)
                    .Contains(cityOccupation.CityId);
        }
        private bool IsMy(Database.Entities.CityOccupation cityOccupation)
        {
            return (cityOccupation.OwnerId == _jwtUser.UserId);
        }
        private bool IsMy(Database.Entities.Order order)
        {
            return (order.UserId == _jwtUser.UserId);
        }

        private int GetCitySize(int army)
        {
            return (int)Math.Sqrt(army) + 10;
        }
        private int GetOrderSize(int army)
        {
            if (army == 0)
                return 0;

            return (int)Math.Sqrt(army) + 5;
        }
        
        private HashSet<Guid> GetVisibleCities(int turnId)
        {
            if (_visibleCities.TryGetValue(turnId, out var visibleCities))
                return visibleCities;

            var friendlyCities = GetFriendlyCities(turnId);
            visibleCities = new HashSet<Guid>(friendlyCities);

            var cityIds = _db.Roads
                .Where(r => friendlyCities.Contains(r.StCityId) || friendlyCities.Contains(r.NdCityId))
                .Select(r => new { r.StCityId, r.NdCityId });
            foreach(var road in cityIds)
            {
                visibleCities.Add(road.StCityId);
                visibleCities.Add(road.NdCityId);
            }

            _visibleCities.Add(turnId, visibleCities);
            return visibleCities;
        }
        private HashSet<Guid> GetFriendlyCities(int turnId)
        {
            if (_friendlyCities.TryGetValue(turnId, out var cities))
                return cities;

            var citiesList = _db.CityOccupations
                .Include(co => co.Owner)
                .Include(co => co.City)
                .Where(co => co.TurnId == turnId && co.Owner.TeamId == _jwtUser.TeamId)
                .Select(co => co.City.Id);
            cities = new HashSet<Guid>(citiesList);
            _friendlyCities.Add(turnId, cities);

            return cities;
        }
    }
}