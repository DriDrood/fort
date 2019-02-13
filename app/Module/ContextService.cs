using System;
using Fort.Database;
using Fort.Database.Entities;

namespace Fort.Module
{
    public class ContextService : IDisposable
    {
        public ContextService()
        {
            Database = new FortDbContext();
        }
        public FortDbContext Database { get; set; }
        public Player CurrentPlayer { get; set; }

        private static Player systemPlayer;

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}