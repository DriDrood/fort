using System;
using System.Text;
using Fort.Database;
using Fort.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fort.Services
{
    public abstract class MapBaseService
    {
        protected MapBaseService(FortDbContext context, CurrentPlayerService currentPlayerService)
        {
            _context = context;
            _currentPlayerService = currentPlayerService;
        }

        protected abstract string GetCityColor(City city);
        protected abstract string GetCityImage(City city);
        protected abstract int? GetCityArmy(City city);

        protected FortDbContext _context;
        protected CurrentPlayerService _currentPlayerService;

        public string Print()
        {
            StringBuilder svgMap = new StringBuilder();

            svgMap.AppendLine($"<svg id=\"map\" viewBox=\"0 0 1600 794\">");
            foreach (Path path in _context.Paths
                .Include(p => p.Source).ThenInclude(c => c.Owner).ThenInclude(u => u.Team)
                .Include(p => p.Target).ThenInclude(c => c.Owner).ThenInclude(u => u.Team))
            {
                var middle = getMiddlePoint(path);
                svgMap.AppendLine($"<line x1=\"{path.Source.X}\" y1=\"{path.Source.Y}\" x2=\"{middle.x}\" y2=\"{middle.y}\" style=\"stroke:{GetCityColor(path.Source)};stroke-width:5\" />");
                svgMap.AppendLine($"<line x1=\"{middle.x}\" y1=\"{middle.y}\" x2=\"{path.Target.X}\" y2=\"{path.Target.Y}\"  style=\"stroke:{GetCityColor(path.Target)};stroke-width:5\" />");
            }

            foreach (City city in _context.Cities)
            {
                svgMap.AppendLine($"<circle cx=\"{city.X}\" cy=\"{city.Y}\" r=\"{System.Math.Log10(city.Army) * 10}\" fill=\"{GetCityColor(city)}\" />");
            }
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

#warning TODO: 
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

        private static (double x, double y) getMiddlePoint(Path path)
        {
            (double x, double y) result = (0, 0);
            result.x = ((path.Target.X - path.Source.X) / 2) + path.Source.X;
            result.y = ((path.Target.Y - path.Source.Y) / 2) + path.Source.Y;

            return result;
        }
    }
}