using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Fort.Module.Army
{
    public abstract class ArmyService
    {
        public ArmyService(ContextService context)
        {
            _context = context;
        }

        protected ContextService _context;

        public JToken GetInit(int roundId)
        {
            return new JObject{
                { "paths", getPaths() },
                { "turns", getTurns(roundId) },
                { "cities", getCities() }
            };
        }
        public Task PlayerTurn(User user, int sourceCityId, int targetCityId, int amount, int roundId)
        {
            var turn = _context.Database.Turns.FirstOrDefault(t => t.RoundId == roundId && t.SourceCityId == sourceCityId && t.TargetCityId == targetCityId);

            // create new
            if (turn == null)
            {
                turn = new Turn
                {
                    UserId = user.Id,
                    SourceCityId = sourceCityId,
                    TargetCityId = targetCityId,
                    CreatedAt = DateTime.UtcNow,
                    RoundId = roundId
                };
                _context.Database.Turns.Add(turn);
            }

            //update
            turn.Amount = amount;
            return _context.Database.SaveChangesAsync();
        }
        public void GetRoundResult()
        {

        }
        public void GetHistory()
        {
            // TODO
        }

        private JToken getPaths()
        {
            var paths = _context.Database.Paths
                .Include(p => p.Source).ThenInclude(s => s.Owner).ThenInclude(o => o.Team)
                .Include(p => p.Target).ThenInclude(t => t.Owner).ThenInclude(o => o.Team)
                .ToList()
                .Select(p =>
                {
                    var city1 = p.SourceId < p.TargetId ? p.Source : p.Target;
                    var city2 = p.SourceId < p.TargetId ? p.Target : p.Source;
                    return new
                    {
                        id = $"{city1.Id}-{city2.Id}",
                        x1 = city1.X,
                        y1 = city1.Y,
                        color1 = GetCityColor(city1),
                        x2 = city2.X,
                        y2 = city2.Y,
                        color2 = GetCityColor(city2)
                    };
                });

            return JToken.FromObject(paths);
        }
        private JToken getTurns(int roundId)
        {
            var turns = GetVisibleTurn().Where(t => t.RoundId == roundId).Select(t => new
            {
                sourceCityId = t.SourceCityId,
                targetCityId = t.TargetCityId,
                amount = t.Amount,
                color = GetTurnColor(t)
            });

            return JToken.FromObject(turns);
        }
        private JToken getCities()
        {
            var cities = _context.Database.Cities
                .Include(c => c.Owner)
                .Include(c => c.SourceToPaths).ThenInclude(s => s.Target).ThenInclude(t => t.Owner)
                .Include(c => c.TargetToPaths).ThenInclude(t => t.Source).ThenInclude(s => s.Owner)
                .ToList()
                .Select(c =>
                {
                    var owner = GetOwner(c);
                    return new
                    {
                        id = c.Id,
                        x = c.X,
                        y = c.Y,
                        r = GetRadius(c.Army),
                        color = GetCityColor(c),
                        neighbours = GetNeighbours(c),
                        owned = IsOwned(c),
                        army = GetArmy(c),
                        ownerId = owner?.Id,
                        image = owner?.ImageUrl
                    };
                });

            return JToken.FromObject(cities);
        }

        private int GetRadius(int army) => (int)(System.Math.Log10(army + 1) * 10) + 2;
        protected bool IsOwned(City city) => _context.CurrentPlayer.IsUser() && city.Owner == _context.CurrentPlayer;
        protected string GetTurnColor(Turn turn) => GetCityColor(turn.SourceCity); // include turn.city.owner.team
        private IEnumerable<int> GetNeighbours(City city)
        {
            foreach (var path in city.SourceToPaths)
                yield return path.TargetId;

            foreach (var path in city.TargetToPaths)
                yield return path.SourceId;
        }

        protected abstract string GetCityColor(City city); // include city.owner
        protected abstract int GetArmy(City city); // -1 for unknown
        protected abstract User GetOwner(City city); // include city.owner
        protected abstract IEnumerable<Turn> GetVisibleTurn();
    }
}