using Fort.Database;
using Fort.Database.Entities;

namespace Fort.Services
{
    public class MapAdminService : MapBaseService
    {
        public MapAdminService(FortDbContext context, Player player) : base(context, player)
        {
        }

        protected override int GetCityArmy(City city)
        {
            return city.Army;
        }

        protected override string GetCityColor(City city)
        {
            return Program.Config.ColorsForAdmin[city.Owner?.Team.Id ?? "neutral"];
        }

        protected override bool ShowCityAvatar(City city)
        {
            if (city.Owner != null)
                return true;

            return false;
        }
    }
}