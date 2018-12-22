using System.Linq;
using Fort.Database;
using Fort.Database.Entities;

namespace Fort.Services
{
    public class MapTeamService : MapBaseService
    {
        public MapTeamService(FortDbContext context, Player player) : base(context, player)
        {
        }

        protected override int GetCityArmy(City city)
        {
            if (city.Owner?.TeamId == _player.Id)
                return city.Army;

            return -1;
        }

        protected override string GetCityColor(City city)
        {
            // neutral
            if (city.Owner?.Team?.Id == null)
                return "gray";

            // ally
            if (city.Owner.TeamId == _player.Id)
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
            if (city.Owner.TeamId == _player.Id)
                return true;

            // city near enemy border
            if (city.Neighbour.Any(c => c.Owner?.TeamId == _player.Id))
                return true;

            return false;
        }
    }
}