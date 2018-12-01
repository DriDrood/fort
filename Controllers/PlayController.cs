using Fort.Database;
using Fort.Database.Entities;
using Fort.Services;
using Fort.Utils.Logger;
using Microsoft.AspNetCore.Mvc;

namespace Fort.Controllers
{
    public class PlayController : Controller
    {
        public PlayController(FortDbContext context, CurrentPlayerService currentPlayerService, MapUserService mapUserService, MapTeamService mapTeamService, MapAdminService mapAdminService)
        {
            _context = context;
            _currentPlayerService = currentPlayerService;
            _mapUserService = mapUserService;
            _mapTeamService = mapTeamService;
            _mapAdminService = mapAdminService;
        }

        private FortDbContext _context;
        private CurrentPlayerService _currentPlayerService;
        private MapUserService _mapUserService;
        private MapAdminService _mapAdminService;
        private MapTeamService _mapTeamService;

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
            if (_currentPlayerService.Player is User)
            {
                if ((_currentPlayerService.Player as User).IsAdmin)
                    return View(_mapAdminService);

                return View(_mapUserService);
            }

            if (_currentPlayerService.Player is Team)
                return View(_mapTeamService);

            throw new FortException(ELogLevel.Warning, "Neplatný kód!");
        }
    }
}