using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Fort.Models;
using Fort.Services;

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
            var round = _mapService.Turn();
            if (round != null)
                foreach (var turn in round)
                    turn.Play();

            return Ok(new
            {
                Round = round,
                Map = Helpers.Map.Print()
            });
        }

        public IActionResult Reset()
        {
            _mapService.Load();

            return RedirectToAction("Index");
        }
    }
}
