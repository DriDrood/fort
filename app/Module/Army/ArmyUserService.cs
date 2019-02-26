using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Fort.Database.Entities;
using Fort.Utils.Logger;

namespace Fort.Module.Army
{
    public class ArmyUserService : ArmyService
    {
        public ArmyUserService(ContextService context) : base(context)
        {
        }

        protected override int GetArmy(City city)
        {
            if (city.Owner == _context.CurrentPlayer
                || city.Owner?.TeamId == _context.CurrentPlayer.GetTeamId())
                return city.Army;

            return -1;
        }

        protected override string GetCityColor(City city)
        {
            if (city.OwnerId == _context.CurrentPlayer.Id)
            {
                var teamColor = hexColorToInt(city.Owner.Team.Color);
                var yourColor = lighter(teamColor, 1.8);

                return $"rgb({yourColor[0]}, {yourColor[1]}, {yourColor[2]})";
            }

            return city.Owner?.Team?.Color ?? "gray";
        }

        protected override User GetOwner(City city)
        {
            // user or team
            if (city.OwnerId == _context.CurrentPlayer.Id
                || city.Owner?.TeamId == _context.CurrentPlayer.GetTeamId())
                return city.Owner;

            // near enemy
            if (city.SourceToPaths.Any(p => p.Target.Owner?.TeamId == _context.CurrentPlayer.GetTeamId())
                || city.TargetToPaths.Any(p => p.Source.Owner?.TeamId == _context.CurrentPlayer.GetTeamId()))
                return city.Owner;

            // other
            return null;
        }

        protected override IEnumerable<Turn> GetVisibleTurn()
        {
            return _context.Database.Turns.Where(t => t.UserId == _context.CurrentPlayer.Id);
        }

        private static int[] hexColorToInt(string color)
        {
            color = color.ToLower();

            if (!Regex.Match(color, "^#[0-9a-f]{6}$").Success)
                throw new FortException(ELogLevel.UnknownException, "Barva je ve špatném formátu, použijte formát '#fff'");

            int[] result = new int[] { 0, 0, 0 };
            for (int i = 0; i < 6; i++)
            {
                var value = (int)color[i + 1] < 60
                    ? (int)color[i + 1] - 48
                    : (int)color[i + 1] - 87;

                if (i % 2 == 0)
                    value *= 16;

                result[i / 2] += value;
            }

            return result;
        }
        private static int[] lighter(int[] color, double coef)
        {
            for (int i = 0; i < color.Length; i++)
            {
                double final = color[i] * coef;
                color[i] = final < 256
                    ? (int)final
                    : 255;
            }

            return color;
        }
    }
}