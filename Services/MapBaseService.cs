using System.Text;
using Fort.Database;
using Fort.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fort.Services
{
    public abstract class MapBaseService
    {
        protected MapBaseService(FortDbContext context, RealPositionService positionService, CurrentPlayerService currentPlayerService)
        {
            _context = context;
            _positionService = positionService;
            _currentPlayerService = currentPlayerService;
        }

        protected abstract string GetCityColor(City city);
        protected abstract string GetCityImage(City city);
        protected abstract int? GetCityArmy(City city);

        protected FortDbContext _context;
        protected RealPositionService _positionService;
        protected CurrentPlayerService _currentPlayerService;

        public string Print()
        {
            StringBuilder svgMap = new StringBuilder();

            svgMap.AppendLine($"<svg id=\"map\" height=\"{_positionService.RealHeight}\" width=\"{_positionService.RealWidth}\">");
            foreach (Path path in _context.Paths
                .Include(p => p.Source).ThenInclude(c => c.Owner).ThenInclude(u => u.Team)
                .Include(p => p.Target).ThenInclude(c => c.Owner).ThenInclude(u => u.Team))
            {
                var middle = getMiddlePoint(path);
                svgMap.AppendLine($"<line x1=\"{_positionService.ToRealWidth(path.Source.X)}\" y1=\"{_positionService.ToRealHeight(path.Source.Y)}\" x2=\"{_positionService.ToRealWidth(middle.x)}\" y2=\"{_positionService.ToRealHeight(middle.y)}\" style=\"stroke:{GetCityColor(path.Source)};stroke-width:5\" />");
                svgMap.AppendLine($"<line x1=\"{_positionService.ToRealWidth(middle.x)}\" y1=\"{_positionService.ToRealHeight(middle.y)}\" x2=\"{_positionService.ToRealWidth(path.Target.X)}\" y2=\"{_positionService.ToRealHeight(path.Target.Y)}\"  style=\"stroke:{GetCityColor(path.Target)};stroke-width:5\" />");
            }

            foreach (City city in _context.Cities)
            {
                svgMap.AppendLine($"<circle cx=\"{_positionService.ToRealWidth(city.X)}\" cy=\"{_positionService.ToRealHeight(city.Y)}\" r=\"{System.Math.Log10(city.Army) * 20}\" fill=\"{GetCityColor(city)}\" />");
            }
            svgMap.AppendLine("</svg>");

            return svgMap.ToString();
        }

        public string Army(Turn turn)
        {
            return $"<div id=\"army{turn.Id}\" class=\"army\" style=\"background-color:{turn.SourceCity};top:{_positionService.ToRealHeight(turn.SourceCity.Y) - 10}px;left:{_positionService.ToRealWidth(turn.SourceCity.X) - 10}px;\"></div>";
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