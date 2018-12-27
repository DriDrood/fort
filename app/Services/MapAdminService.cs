using System.Linq;
using System.Text;
using Fort.Database;
using Fort.Database.Entities;

namespace Fort.Services
{
    public class MapAdminService : MapBaseService
    {
        public MapAdminService(FortDbContext context, Player player) : base(context, player)
        {
        }

        private CommService _commService => Program.GetService<CommService>();

        public override string ShowStatistics()
        {
            StringBuilder stat = new StringBuilder();
            stat.AppendLine("<div class=\"statistics\">");
            foreach (Team team in _context.Teams)
            {
                stat.AppendLine($"<div class=\"team\" style=\"color:{Program.Config.ColorsForAdmin[team.Id]}\">");
                stat.AppendLine($"<div class=\"team_name\" data-playerId=\"{team.Id}\"><i class=\"fa {(_commService.IsPlayerConnected(team.Id) ? "fa-globe" : "fa-chain-broken")}\"></i> {team.Name}: {team.Members.Sum(m => m.Cities.Count())} - {team.Members.Sum(m => m.Cities.Sum(c => c.Army))}</div>");

                foreach (User member in team.Members)
                {
                    stat.AppendLine($"<div class=\"player\" data-playerId=\"{member.Id}\"><i class=\"fa {(_commService.IsPlayerConnected(member.Id) ? "fa-globe" : "fa-chain-broken")}\"></i> {member.Name}: {member.Cities.Count()} - {member.Cities.Sum(c => c.Army)}</div>");
                }

                stat.AppendLine($"</div>");
            }
            stat.AppendLine("</div>");

            return stat.ToString();
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