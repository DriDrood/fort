using System;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Module.Army;

namespace Fort.Module
{
    public class ContextService : IDisposable
    {
        public ContextService()
        {
            Database = new FortDbContext();
            CurrentPlayer = new User
            {
                Id = "Sys",
                Name = "System",
                IsAdmin = true
            };
        }
        public FortDbContext Database { get; private set; }
        public Player CurrentPlayer { get; set; }
        public ArmyService ArmyService
        {
            get
            {
                if (_armyService == null)
                {
                    if (CurrentPlayer == null)
                        return null;

                    if (CurrentPlayer is User)
                    {
                        if ((CurrentPlayer as User).IsAdmin)
                            _armyService = new ArmyAdminService(this);
                        else
                            _armyService = new ArmyUserService(this);
                    }

                    else if (CurrentPlayer is Team)
                        _armyService = new ArmyTeamService(this);

                    else
                        throw new InvalidOperationException($"Invalid player type: {CurrentPlayer.GetType().FullName}");
                }

                return _armyService;
            }
        }

        private ArmyService _armyService;

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}