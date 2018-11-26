using Fort.Database;
using Fort.Database.Entities;

namespace Fort.Services
{
    public class MapUserService : MapBaseService
    {
        public MapUserService(FortDbContext context, RealPositionService positionService, CurrentPlayerService currentPlayerService) : base(context, positionService, currentPlayerService)
        {
        }

        protected override int? GetCityArmy(City city)
        {
            if (city.Owner?.TeamId == _currentPlayerService.User.TeamId)
                return city.Army;

            return null;
        }

        protected override string GetCityColor(City city)
        {
            // neutral
            if (city.Owner?.Team?.Id == null)
                return "gray";

            // me
            if (city.OwnerId == _currentPlayerService.User.Id)
                return "#2689c2";

            // ally
            if (city.Owner.TeamId == _currentPlayerService.User.TeamId)
                return "#0a8524";

            // enemy
            else
                return "#83001c";
        }

        protected override string GetCityImage(City city)
        {
            // ally
            if (city.Owner.TeamId == _currentPlayerService.User.TeamId)
                return city.Owner.ImageUrl;

            #warning TODO: near cities

            return null;
        }
    }
}