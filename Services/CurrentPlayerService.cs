using Fort.Database.Entities;

namespace Fort.Services
{
    public class CurrentPlayerService
    {
        public User User { get; set; }
        public Team Team { get; set; }

        public override string ToString()
        {
            if (User != null)
                return $"U:{User.Id}";

            if (Team != null)
                return $"T:{Team.Id}";

            return "";
        }
    }
}