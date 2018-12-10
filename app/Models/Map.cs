using System.Collections.Generic;
using Fort.Database.Entities;

namespace Fort.Models
{
    public class Map
    {
        public IEnumerable<City> Cities { get; set; }
        public IEnumerable<Path> Paths { get; set; }
    }
}