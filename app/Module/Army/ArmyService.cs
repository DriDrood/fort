using System.Collections.Generic;
using System.Linq;
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

        private ContextService _context;

        public JToken GetInit()
        {
            return new JObject{
                { "paths", getPaths() },
                { "turns", getTurns() },
                { "cities", getCities() }
            };
        }
        public void PlayerTurn() { }
        public void GetRoundResult() { }
        public void GetHistory() { }

        private JToken getPaths()
        {
            var paths = _context.Database.Paths
                .Include(p => p.Source)
                .Include(p => p.Target)
                .Select(p => new
                {
                    id = p.SourceId > p.TargetId ? $"{p.TargetId}-{p.SourceId}" : $"{p.SourceId}-{p.TargetId}",
                    x1 = p.Source.X,
                    y1 = p.Source.Y,
                    color1 = GetCityColor(p.Source),
                    x2 = p.Target.X,
                    y2 = p.Target.Y,
                    color2 = GetCityColor(p.Target)
                });

            return JToken.FromObject(paths);
        }
        private JToken getTurns()
        {
            var turns = GetVisibleTurn().Select(t => new {
                sourceCityId = t.SourceCityId,
                targetCityId = t.TargetCityId,
                amount = t.Amount,
                color = GetTurnColor(t)
            });

            return JToken.FromObject(turns);
        }
        private JToken getCities()
        {
            var cities = _context.Database.Cities.Select(c => new
            {
                id = c.Id,
                x = c.X,
                y = c.Y,
                r = GetRadius(c.Army),
                color = GetCityColor(c),
                neighbours = GetNeighbours(c),
                owned = IsOwned(c),
                army = GetArmy(c),
                image = GetImage(c)
            });

            return JToken.FromObject(cities);
        }

        private int GetRadius(int army) => (int)(System.Math.Log10(army + 1) * 10) + 2;
        private IEnumerable<int> GetNeighbours(City city)
        {
            foreach (var path in city.SourceToPaths)
                yield return path.TargetId;

            foreach (var path in city.TargetToPaths)
                yield return path.SourceId;
        }

        protected abstract bool IsOwned(City city);
        protected abstract string GetCityColor(City city);
        protected abstract string GetTurnColor(Turn turn);
        protected abstract int GetArmy(City city);
        protected abstract int GetImage(City city);
        protected abstract IEnumerable<Turn> GetVisibleTurn();
    }
}