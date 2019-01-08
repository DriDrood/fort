using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fort;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Services;
using Fort.Utils.Logger;
using Microsoft.EntityFrameworkCore;

namespace Fort.Services
{
    public abstract class MapBaseService
    {
        protected MapBaseService(FortDbContext context, Player player)
        {
            _context = context;
            _player = player;
            _userImages = new HashSet<(User user, int round)>();
        }

        public virtual string ShowStatistics() => string.Empty;

        protected abstract string GetCityColor(City city);
        protected abstract int GetCityArmy(City city);
        protected abstract bool ShowCityAvatar(City city);
        protected virtual string GetCityFill(City city)
        {
            if (ShowCityAvatar(city))
            {
                _userImages.Add((city.Owner, GetRadius(city.Army)));
                return $"url(#U_{city.Owner.Id}_{GetRadius(city.Army)})";
            }

            return GetCityColor(city);
        }

        protected HashSet<(User user, int round)> _userImages;

        protected FortDbContext _context;
        protected Player _player;

        public string Print()
        {
            StringBuilder svgMap = new StringBuilder();

            svgMap.AppendLine($"<svg id=\"map\" viewBox=\"0 0 1600 794\">");
            svgMap.AppendLine("<style> .armyText { font-size: 18px; } </style>");

            // paths
            foreach (Path path in _context.Paths
                .Include(p => p.Source).ThenInclude(c => c.Owner).ThenInclude(u => u.Team)
                .Include(p => p.Target).ThenInclude(c => c.Owner).ThenInclude(u => u.Team))
            {
                var middle = GetMiddlePoint(path);
                svgMap.AppendLine($"<line x1=\"{path.Source.X}\" y1=\"{path.Source.Y}\" x2=\"{middle.x}\" y2=\"{middle.y}\" data-source-id=\"{path.SourceId}\" data-target-id=\"{path.TargetId}\" style=\"stroke:{GetCityColor(path.Source)};stroke-width:5\" />");
                svgMap.AppendLine($"<line x1=\"{middle.x}\" y1=\"{middle.y}\" x2=\"{path.Target.X}\" y2=\"{path.Target.Y}\" data-source-id=\"{path.SourceId}\" data-target-id=\"{path.TargetId}\" style=\"stroke:{GetCityColor(path.Target)};stroke-width:5\" />");
            }

            // cities
            foreach (City city in _context.Cities)
            {
                int cityArmy = GetCityArmy(city);
                if (cityArmy > 0)
                {
                    svgMap.AppendLine($"<circle cx=\"{city.X - GetRadius(city.Army)}\" cy=\"{city.Y - GetRadius(city.Army)}\" r=\"14\" fill=\"white\" style=\"stroke:black;stroke-width:2;\" />");
                    svgMap.AppendLine($"<text x=\"{city.X - GetRadius(city.Army)}\" y=\"{city.Y - GetRadius(city.Army) + 6}\" text-anchor=\"middle\" class=\"armyText\">{cityArmy}</text>");
                }
                svgMap.AppendLine($"<circle cx=\"{city.X}\" cy=\"{city.Y}\" r=\"{GetRadius(city.Army)}\" data-city-id=\"{city.Id}\" data-neighbours=\"{string.Join(" ", city.Neighbour.Select(c => c.Id))}\" data-owned=\"{city.OwnerId == _player.Id}\" data-army=\"{(cityArmy)}\" fill=\"{GetCityFill(city)}\" style=\"stroke:{GetCityColor(city)};stroke-width:2\" />");
                svgMap.AppendLine($"<circle cx=\"{city.X}\" cy=\"{city.Y}\" r=\"25\" data-for=\"{city.Id}\" fill-opacity=\"0\" />");
            }
            // users image
            svgMap.AppendLine($"<defs>");

            svgMap.AppendLine("<filter id=\"shadow\"  filterUnits=\"userSpaceOnUse\">");
            svgMap.AppendLine("<feDropShadow dx=\"0\" dy=\"0\" stdDeviation=\"5\" flood-color=\"rgb(0, 173, 204)\" flood-opacity=\"1\" />");
            svgMap.AppendLine("</filter>");
            foreach ((User user, int round) in _userImages)
            {
                svgMap.AppendLine($"<pattern id=\"U_{user.Id}_{round}\" width=\"1\" height=\"1\">");
                svgMap.AppendLine($"<image xlink:href=\"/images/Users/{user.ImageUrl}\" x=\"0\" y=\"0\" width=\"{round * 2}\" height=\"{round * 2}\" />");
                svgMap.AppendLine($"</pattern>");
            }
            svgMap.AppendLine($"</defs>");

            // end
            svgMap.AppendLine("</svg>");

            return svgMap.ToString();
        }

        public string GetStatus()
        {
            string result = "";
            switch (Program.GetService<RoundService>().State)
            {
                case Fort.Utils.Timer.Status.Begin:
                    result = "<i class=\"fa fa-circle-o-notch\" title=\"Začíná kolo\"></i>";
                    break;
                case Fort.Utils.Timer.Status.Running:
                    result = "<i class=\"fa fa-play\" title=\"Kolo běží\"></i>";
                    break;
                case Fort.Utils.Timer.Status.Paused:
                    result = "<i class=\"fa fa-pause\" title=\"Kolo je pozastavené\"></i>";
                    break;
                case Fort.Utils.Timer.Status.Finished:
                    result = "<i class=\"fa fa-flag-checkered\" title=\"Kolo skončilo\"></i>";
                    break;

                default:
                    throw new InvalidOperationException("Neznámý stav kola");
            }

            return $"{result} Kolo {Program.GetService<RoundService>().CurrentRound.RoundNumber}";
        }
        public string GetRemaining()
        {
            var rem = Program.GetService<RoundService>().Remaining;
            if (rem == null)
                return "0:00:00";

            return $"{(int)rem.Value.TotalHours}:{rem.Value.Minutes.ToString().PadLeft(2, '0')}:{rem.Value.Seconds.ToString().PadLeft(2, '0')}";
        }

        public string[] Army(Turn turn)
        {
            // not changed
            if (turn.Amount == turn.ModifiedAmount)
                return
                    new string[] { $"<div id=\"army{turn.Id}\" class=\"army\" data-time=\"all\" data-final-x=\"{turn.TargetCity.X}\" data-final-y=\"{turn.TargetCity.Y}\" style=\"width:{GetRadius(turn.Amount)}px;height:{GetRadius(turn.Amount)}px;border-radius:{GetRadius(turn.Amount)}px;background-color:{GetCityColor(turn.SourceCity)};top:{turn.SourceCity.Y}px;left:{turn.SourceCity.X}px;\"></div>" };

            var middle = GetMiddlePoint(turn);

            // get smaller
            if (turn.ModifiedAmount > 0)
                return new string[]
                {
                    $"<div id=\"army{turn.Id}a\" class=\"army\" data-time=\"begin\" data-final-x=\"{middle.x}\" data-final-y=\"{middle.y}\" style=\"width:{GetRadius(turn.Amount)}px;height:{GetRadius(turn.Amount)}px;border-radius:{GetRadius(turn.Amount)}px;background-color:{GetCityColor(turn.SourceCity)};top:{turn.SourceCity.Y}px;left:{turn.SourceCity.X}px;\"></div>",
                    $"<div id=\"army{turn.Id}b\" class=\"army\" data-time=\"end\" data-final-x=\"{turn.TargetCity.X}\" data-final-y=\"{turn.TargetCity.Y}\" style=\"width:{GetRadius(turn.ModifiedAmount.Value)}px;height:{GetRadius(turn.ModifiedAmount.Value)}px;border-radius:{GetRadius(turn.ModifiedAmount.Value)}px;background-color:{GetCityColor(turn.SourceCity)};top:{middle.y}px;left:{middle.x}px;\"></div>"
                };

            // destroyed
            return
                new string[] { $"<div id=\"army{turn.Id}\" class=\"army\" data-time=\"begin\" data-final-x=\"{middle.x}\" data-final-y=\"{middle.y}\" style=\"width:{GetRadius(turn.Amount)}px;height:{GetRadius(turn.Amount)}px;border-radius:{GetRadius(turn.Amount)}px;background-color:{GetCityColor(turn.SourceCity)};top:{turn.SourceCity.Y}px;left:{turn.SourceCity.X}px;\"></div>" };
        }

        public static (double x, double y) GetMiddlePoint(Path path)
        {
            return GetMiddlePoint(path.Source.X, path.Source.Y, path.Target.X, path.Target.Y);
        }
        public static (double x, double y) GetMiddlePoint(Turn turn)
        {
            return GetMiddlePoint(turn.SourceCity.X, turn.SourceCity.Y, turn.TargetCity.X, turn.TargetCity.Y);
        }
        private static (double x, double y) GetMiddlePoint(double sourceX, double sourceY, double targetX, double targetY)
        {
            (double x, double y) result = (0, 0);
            result.x = ((targetX - sourceX) / 2) + sourceX;
            result.y = ((targetY - sourceY) / 2) + sourceY;

            return result;
        }

        public int GetRadius(int army) => (int)(army == 0 ? 3 : (System.Math.Log10(army) * 10 + 2));

        public static MapBaseService GetMapServiceForPlayer(FortDbContext context, Player player)
        {
            if (player is User)
            {
                if ((player as User).IsAdmin)
                    return new MapAdminService(context, player);

                return new MapUserService(context, player);
            }

            if (player is Team)
                return new MapTeamService(context, player);

            throw new FortException(ELogLevel.Warning, "Neplatný kód!");
        }
    }
}