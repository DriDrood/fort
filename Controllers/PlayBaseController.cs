using Fort.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fort.Controllers
{
    public abstract class PlayBaseController : Controller
    {
        protected abstract MapBaseService _mapService { get; }
        protected abstract bool _existThisCode(string code);

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string code)
        {
            if (_existThisCode(code))
                return RedirectToAction("Map", new { code = code });

            ViewData["errorMessage"] = "Neplatný kód!";
            return View();
        }

        public IActionResult Map(string code)
        {
            return View(_mapService);
        }
    }
}