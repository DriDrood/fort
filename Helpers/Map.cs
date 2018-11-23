using System.Text;
using Fort.Models;
using Fort.Services;

namespace Fort.Helpers
{
    public static class Map
    {
        public static string Print()
        {
            StringBuilder svgMap = new StringBuilder();
            MapService mapService = Startup.Get<MapService>();

            svgMap.AppendLine($"<svg id=\"map\" height=\"{Position.RealHeight}\" width=\"{Position.RealWidth}\">");
            foreach (Path path in mapService.Paths)
            {
                var middle = getMiddlePoint(path);
                svgMap.AppendLine($"<line x1=\"{Position.ToRealWidth(path.Source.X)}\" y1=\"{Position.ToRealHeight(path.Source.Y)}\" x2=\"{Position.ToRealWidth(middle.x)}\" y2=\"{Position.ToRealHeight(middle.y)}\" style=\"stroke:{path.Source.Owner.Color};stroke-width:5\" />");
                svgMap.AppendLine($"<line x1=\"{Position.ToRealWidth(middle.x)}\" y1=\"{Position.ToRealHeight(middle.y)}\" x2=\"{Position.ToRealWidth(path.Target.X)}\" y2=\"{Position.ToRealHeight(path.Target.Y)}\"  style=\"stroke:{path.Target.Owner.Color};stroke-width:5\" />");
            }

            foreach (Fortress fortress in mapService.Fortresses.Values)
            {
                svgMap.AppendLine($"<circle cx=\"{Position.ToRealWidth(fortress.X)}\" cy=\"{Position.ToRealHeight(fortress.Y)}\" r=\"{System.Math.Log10(fortress.Population)*20}\" fill=\"{fortress.Owner.Color}\" />");
            }
            svgMap.AppendLine("</svg>");

            return svgMap.ToString();
        }

        private static (double x, double y) getMiddlePoint(Path path)
        {
            (double x, double y) result = (0, 0);
            result.x = ((path.Target.X - path.Source.X) / 2) + path.Source.X;
            result.y = ((path.Target.Y - path.Source.Y) / 2) + path.Source.Y;

            return result;
        }

        public static string Army(Turn turn)
        {
            return $"<div id=\"army{turn.Id}\" class=\"army\" style=\"background-color:{turn.From.Owner.Color};top:{Position.ToRealHeight(turn.From.Y)-10}px;left:{Position.ToRealWidth(turn.From.X)-10}px;\"></div>";
        }
    }
}