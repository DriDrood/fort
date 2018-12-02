using Fort.Database;
using Fort.Database.Entities;

namespace Fort.Services
{
    public class MapTeamService : MapBaseService
    {
        public MapTeamService(FortDbContext context, CurrentPlayerService currentPlayerService) : base(context, currentPlayerService)
        {
        }

        protected override int? GetCityArmy(City city)
        {
            if (city.Owner?.TeamId == _currentPlayerService.Team.Id)
                return city.Army;

            return null;
        }

        protected override string GetCityColor(City city)
        {
            // neutral
            if (city.Owner?.Team?.Id == null)
                return "gray";

            // ally
            if (city.Owner.TeamId == _currentPlayerService.Team.Id)
                return "#0a8524";

            // enemy
            else
                return "#83001c";
        }

        protected override string GetCityImage(City city)
        {
            // ally
            if (city.Owner.TeamId == _currentPlayerService.Team.Id)
                return city.Owner.ImageUrl;

            #warning TODO: near cities

            return null;
        }
    }
}