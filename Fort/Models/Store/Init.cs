using System;
using System.Collections.Generic;

namespace Fort.Models.Store
{
    public class Init
    {
        public CurrentTurn CurrentTurn { get; set; }
        public Dictionary<Guid, City> Cities { get; set; }
        public IEnumerable<string> Roads { get; set; }
        public Dictionary<Guid, Player> Players { get; set; }
        public Dictionary<Guid, Team> Teams { get; set; }
        public Config Config { get; set; }
    }
}