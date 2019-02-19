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

        public Army.ArmyService GetArmyService()
        {
            if (CurrentPlayer == null)
                return null;

            if (CurrentPlayer is User)
            {
                if ((CurrentPlayer as User).IsAdmin)
                    return new Army.ArmyAdminService(this);

                return new Army.ArmyUserService(this);
            }

            if (CurrentPlayer is Team)
                return new Army.ArmyTeamService(this);

            throw new InvalidOperationException($"Invalid player type: {CurrentPlayer.GetType().FullName}");
        }

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}