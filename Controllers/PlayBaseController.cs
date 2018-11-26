using Fort.Database;
using Fort.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fort.Controllers
{
    public abstract class PlayBaseController : Controller
    {
        protected abstract MapBaseService MapService { get; }
        protected abstract bool ExistThisCode(string code);
        protected abstract void Set(string code);

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string code)
        {
            if (ExistThisCode(code))
                return RedirectToAction("Map", new { code = code });

            ViewData["errorMessage"] = "Neplatný kód!";
            return View();
        }

        public IActionResult Map(string code)
        {
            Set(code);
            return View(MapService);
        }
    }
}