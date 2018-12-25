using Fort.Database.Entities;

namespace Fort.Services
{
    public class CurrentPlayerService
    {
        public Player Player { get; set; }

        public User User => (Player is User) ? (User)Player : null;
        public Team Team => (Player is Team) ? (Team)Player : null;

        public override string ToString()
        {
            return Player?.Id ?? "";
        }
    }
}