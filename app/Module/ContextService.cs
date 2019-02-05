using Fort.Database;
using Fort.Database.Entities;

namespace app.Module
{
    public class ContextService
    {
        public FortDbContext Database { get; set; }
        public Player CurrentPlayer { get; set; }
    }
}