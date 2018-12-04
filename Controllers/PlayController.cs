using Fort.Database;
using Fort.Database.Entities;
using Fort.Services;
using Fort.Utils.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Fort.Controllers
{
    public class PlayController : Controller
    {
        public PlayController(FortDbContext context, CurrentPlayerService currentPlayerService)
        {
            _context = context;
            _currentPlayerService = currentPlayerService;
        }

        private FortDbContext _context;
        private CurrentPlayerService _currentPlayerService;

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string code)
        {
            Player player = (Player)_context.Users.Find(code)
                ?? _context.Teams.Find(code);

            if (player != null)
                return RedirectToAction("Map", new { code = code });

            Logger.Log(ELogLevel.Warning, code, "Neplatný kód!");
            ViewData["errorMessage"] = "Neplatný kód!";
            return View();
        }

        public IActionResult Map(string code)
        {
            ViewData["player"] = _currentPlayerService;
            return View(MapBaseService.GetMapServiceForPlayer(_context, _currentPlayerService.Player));
        }
    }
}