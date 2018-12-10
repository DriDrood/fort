using Fort.Database.Entities;

namespace Fort.Services
{
    public class CurrentPlayerService
    {
        public Player Player { get; set; }

        public User User => (User)Player;
        public Team Team => (Team)Player;

        public override string ToString()
        {
            return Player?.Id ?? "";
        }
    }
}