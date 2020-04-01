using System;
using System.Collections.Generic;
using System.Linq;
using Fort.Database;
using Fort.Models;

namespace Fort.Managers
{
    public class MapManager
    {
        public MapManager(FortDbContext db)
        {
            _db = db;
        }

        private FortDbContext _db;

        public Dictionary<Guid, City> GetAllCities()
        {
            var cities = _db.Cities.ToDictionary(c => c.Id, c => new City {
                Id = c.Id,
                X = c.X,
                Y = c.Y
            });
            return cities;
        }
        public IEnumerable<string> GetAllRoads()
        {
            var roads = _db.Roads
                .Select(r => string.Compare(r.SourceId.ToString(), r.TargetId.ToString()) < 0 ? $"{r.SourceId}__{r.TargetId}" : $"{r.TargetId}__{r.SourceId}")
                .ToArray();
            return roads;
        }
    }
}