using System.Linq;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Models;
using Fort.Utils.Logger;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Fort.Controllers
{
    public class MapCreatorController : Controller
    {
        public IActionResult Index()
        {
            using (FortDbContext context = new FortDbContext())
            {
                ViewData["page"] = "mapCreator";
                Map map = new Map
                {
                    Cities = context.Cities.ToList(),
                    Paths = context.Paths.ToList()
                };
                return View(map);
            }
        }


        [HttpPost]
        public string AddPoint([FromBody]JToken coord)
        {
            int x = coord["x"].Value<int>();
            int y = coord["y"].Value<int>();

            using (FortDbContext context = new FortDbContext())
            {
                var city = new City
                {
                    X = x,
                    Y = y,
                    Army = 5
                };
                context.Cities.Add(city);
                context.SaveChanges();
            }

            return "ok";
        }

        [HttpPost]
        public string DeletePoint([FromBody]JToken coord)
        {
            int x = coord["x"].Value<int>();
            int y = coord["y"].Value<int>();

            using (FortDbContext context = new FortDbContext())
            {
                var city = context.Cities.SingleOrDefault(c => c.X == x && c.Y == y) ?? throw new FortException(ELogLevel.Warning, "Město nenalezeno");
                context.Cities.Remove(city);
                context.SaveChanges();
            }

            return "ok";
        }

        [HttpGet]
        public IActionResult Paths()
        {
            using (FortDbContext context = new FortDbContext())
            {
                ViewData["page"] = "mapCreator_paths";
                Map map = new Map
                {
                    Cities = context.Cities.ToList(),
                    Paths = context.Paths.ToList()
                };
                return View("index", map);
            }
        }

        [HttpPost]
        public string Paths([FromBody]JToken coords)
        {
            using (FortDbContext context = new FortDbContext())
            {
                City sourceCity = context.Cities.FirstOrDefault(c => c.X == coords["source"]["x"].Value<int>() && c.Y == coords["source"]["y"].Value<int>()) ?? throw new FortException(ELogLevel.Warning, "Zdrojové město nenalezeno");
                City targetCity = context.Cities.FirstOrDefault(c => c.X == coords["target"]["x"].Value<int>() && c.Y == coords["target"]["y"].Value<int>()) ?? throw new FortException(ELogLevel.Warning, "Cílové město nenalezeno");

                Path path = context.Paths.FirstOrDefault(p => p.SourceId == sourceCity.Id && p.TargetId == targetCity.Id);
                if (path == null)
                    context.Paths.Add(new Path
                    {
                        Source = sourceCity,
                        Target = targetCity
                    });
                else
                    context.Paths.Remove(path);

                context.SaveChanges();

                return "ok";
            }
        }
    }
}