using System;
using System.Linq;
using Fort.Database;
using Fort.Database.Entities;
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
                return View(context.Cities.ToList());
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
                    Y = y
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
                var city = context.Cities.SingleOrDefault(c => c.X == x && c.Y == y) ?? throw new Exception("City not found");
                context.Cities.Remove(city);
                context.SaveChanges();
            }

            return "ok";
        }
    }
}