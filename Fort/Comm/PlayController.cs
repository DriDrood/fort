using Fort.Managers;
using Fort.Models;
using Fort.Models.Store;
using Fort.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fort.Comm
{
    public class PlayController : Controller
    {
        public PlayController(Context context, DoneService doneService, LifecycleService lifecycleService, MapManager mapManager, TurnManager turnManager, UserManager userManager)
        {
            _context = context;
            _doneService = doneService;
            _lifecycleService = lifecycleService;
            _mapManager = mapManager;
            _turnManager = turnManager;
            _userManager = userManager;
        }

        private readonly Context _context;
        private readonly DoneService _doneService;
        private readonly LifecycleService _lifecycleService;
        private readonly MapManager _mapManager;
        private readonly TurnManager _turnManager;
        private readonly UserManager _userManager;

        public ActionResult Login([FromBody]LoginData loginData)
        {
            // authentication
            var login = _userManager.Login(loginData.Email, loginData.Password);
            if (login == null)
                return Unauthorized();

            // init
            var init = GetInitData();
            init.Login = login;
            return Ok(init);
        }

        [Authorize]
        public ActionResult Init()
        {
            var init = GetInitData();
            return Ok(init);
        }

        [Authorize]
        public ActionResult GetTurn([FromBody]TurnData data)
        {
            var turn = _turnManager.GetTurn(data.Id);
            return Ok(turn);
        }

        [Authorize]
        public ActionResult TurnDone([FromBody]DoneData data)
        {
            _doneService.Done(_context.CurrentUser.Id, data.Done);

            return Ok();
        }

        [Authorize]
        public ActionResult SetOrder([FromBody]OrderData data)
        {
            _turnManager.SetOrder(data, _context.CurrentUser.Id, _lifecycleService.CurrentTurnId);

            return Ok();
        }

        private Init GetInitData()
        {
            var initData = new Init();
            initData.CurrentTurn = _turnManager.GetCurrentTurn();
            initData.Cities = _mapManager.GetAllCities();
            initData.Roads = _mapManager.GetAllRoads();
            initData.Players = _userManager.GetAllPlayers();
            initData.Teams = _userManager.GetAllTeams();
            initData.Config = ConfigManager.Config;

            return initData;
        }
    }
}