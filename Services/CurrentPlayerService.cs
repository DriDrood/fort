using Fort.Database.Entities;

namespace Fort.Services
{
    public class CurrentPlayerService
    {
        public User User { get; set; }
        public Team Team { get; set; }

        public override string ToString()
        {
            return User?.Id ?? Team?.Id ?? "";
        }
    }
}