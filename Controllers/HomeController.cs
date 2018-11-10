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
    }
}
