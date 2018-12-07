using System.Linq;
using Fort.Database;
using Fort.Database.Entities;

namespace Fort.Services
{
    public class MapUserService : MapBaseService
    {
        public MapUserService(FortDbContext context, Player player) : base(context, player)
        {
        }

        protected override int? GetCityArmy(City city)
        {
            if (city.Owner?.TeamId == (_player as User).TeamId)
                return city.Army;

            return null;
        }

        protected override string GetCityColor(City city)
        {
            // neutral
            if (city.Owner?.Team?.Id == null)
                return "gray";

            // me
            if (city.OwnerId == _player.Id)
                return "#2689c2";

            // ally
            if (city.Owner.TeamId == (_player as User).TeamId)
                return "#0a8524";

            // enemy
            else
                return "#83001c";
        }

        protected override bool ShowCityAvatar(City city)
        {
            // neutral
            if (city.Owner == null)
                return false;

            // ally
            if (city.Owner?.TeamId == (_player as User).TeamId)
                return true;

            // city near enemy border
            if (city.Neighbour.Any(c => c.Owner?.TeamId == (_player as User).TeamId))
                return true;

            return false;
        }
    }
}