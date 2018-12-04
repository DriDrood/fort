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

        protected abstract string GetCityColor(City city);
        protected abstract int? GetCityArmy(City city);
        protected abstract bool ShowCityAvatar(City city);
        protected virtual string GetCityFill(City city)
        {
            if (ShowCityAvatar(city))
            {
                _userImages.Add((city.Owner, city.Radius));
                return $"url(#U_{city.Owner.Id}_{city.Radius})";
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

            // paths
            foreach (Path path in _context.Paths
                .Include(p => p.Source).ThenInclude(c => c.Owner).ThenInclude(u => u.Team)
                .Include(p => p.Target).ThenInclude(c => c.Owner).ThenInclude(u => u.Team))
            {
                var middle = GetMiddlePoint(path);
                svgMap.AppendLine($"<line x1=\"{path.Source.X}\" y1=\"{path.Source.Y}\" x2=\"{middle.x}\" y2=\"{middle.y}\" style=\"stroke:{GetCityColor(path.Source)};stroke-width:5\" />");
                svgMap.AppendLine($"<line x1=\"{middle.x}\" y1=\"{middle.y}\" x2=\"{path.Target.X}\" y2=\"{path.Target.Y}\"  style=\"stroke:{GetCityColor(path.Target)};stroke-width:5\" />");
            }

            // cities
            foreach (City city in _context.Cities)
            {
                svgMap.AppendLine($"<circle cx=\"{city.X}\" cy=\"{city.Y}\" r=\"{city.Radius}\" data-city-id=\"{city.Id}\" data-army=\"{city.Army}\" fill=\"{GetCityFill(city)}\" style=\"stroke:{GetCityColor(city)};stroke-width:2\" />");
            }
            // users image
            svgMap.AppendLine($"<defs>");
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

            return $"{rem.Value.Hours}:{rem.Value.Minutes.ToString().PadLeft(2, '0')}:{rem.Value.Seconds.ToString().PadLeft(2, '0')}";
        }

        public string Army(Turn turn)
        {
            return $"<div id=\"army{turn.Id}\" class=\"army\" style=\"background-color:{turn.SourceCity};top:{turn.SourceCity.Y - 10}px;left:{turn.SourceCity.X - 10}px;\"></div>";
        }

        public static (double x, double y) GetMiddlePoint(Path path)
        {
            return GetMiddlePoint(path.Source.X, path.Source.Y, path.Target.X, path.Target.Y);
        }
        public static (double x, double y) GetMiddlePoint(double sourceX, double sourceY, double targetX, double targetY)
        {
            (double x, double y) result = (0, 0);
            result.x = ((targetX - sourceX) / 2) + sourceX;
            result.y = ((targetY - sourceY) / 2) + sourceY;

            return result;
        }

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