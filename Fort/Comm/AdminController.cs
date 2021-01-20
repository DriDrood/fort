using Fort.Database;
using Fort.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fort.Comm
{
    // [Authorize("Admin")]
    public class AdminController : Controller
    {
        public AdminController(FortDbContext db, LifecycleService lifecycleService)
        {
            _db = db;
            _lifecycleService = lifecycleService;
        }

        private readonly FortDbContext _db;
        private readonly LifecycleService _lifecycleService;

        public ActionResult StartGame()
        {
            _lifecycleService.StartGame(_db);
            return Ok();
        }
        public ActionResult ResetGame()
        {
            _lifecycleService.ResetGame(_db);

            return Ok();
        }
        public ActionResult PauseTurn()
        {
            _lifecycleService.PauseTurn(_db);

            return Ok();
        }
        public ActionResult ResumeTurn()
        {
            _lifecycleService.ResumeTurn(_db);

            return Ok();
        }
        public ActionResult EndTurn()
        {
            _lifecycleService.EndTurn(_db);

            return Ok();
        }
    }
}