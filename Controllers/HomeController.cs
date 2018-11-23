using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Fort.Models;
using Fort.Services;
using Newtonsoft.Json.Linq;
using Fort.Database;

namespace Fort.Controllers
{
    public class HomeController : Controller
    {
        private MapService _mapService => Startup.Get<MapService>();

        public IActionResult Index()
        {
            return View(_mapService);
        }

        public IActionResult Turn()
        {
            var round = _mapService.Turn() ?? new Turn[] { };

            foreach (var turn in round)
                turn.Play();

            foreach (Fortress fortress in _mapService.Fortresses.Values)
                fortress.Population += _mapService.PopulationGrow;

            return Ok(new
            {
                Round = round?.Select(t => new { id = t.Id, element = Helpers.Map.Army(t), finalx = Helpers.Position.ToRealWidth(t.To.X) - 10, finaly = Helpers.Position.ToRealHeight(t.To.Y) - 10 }),
                Map = Helpers.Map.Print()
            });
        }

        public IActionResult Reset()
        {
            _mapService.Load();

            return RedirectToAction("Index");
        }

        public IActionResult CreateMap()
        {
            using (FortDbContext context = new FortDbContext())
            {
                return View(context.Fortresses.ToList());
            }
        }

        [HttpPost]
        public string CreateMap([FromBody]JArray coords)
        {
            using (FortDbContext context = new FortDbContext())
            {
                context.Fortresses.RemoveRange(context.Fortresses);

                foreach (JToken coord in coords)
                {
                    context.Fortresses.Add(new Database.Entities.Fortress
                    {
                        X = coord["x"].Value<int>(),
                        Y = coord["y"].Value<int>()
                    });
                }

                context.SaveChanges();
            }

            return "ok";
        }
    }
}
