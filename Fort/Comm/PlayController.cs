using Fort.Managers;
using Fort.Models.Params;
using Fort.Models.Store;
using Fort.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fort.Comm
{
    [Authorize]
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

        /// Login and get initial state
        [AllowAnonymous]
        public ActionResult Login([FromBody]LoginParams param)
        {
            // authentication
            var login = _userManager.Login(param.Email, param.Password);
            if (login == null)
                return Unauthorized();

            // init
            var init = GetInitData();
            init.Login = login;
            return Ok(init);
        }

        /// Get Initial state
        public ActionResult Init()
        {
            var init = GetInitData();
            return Ok(init);
        }

        /// Check state - not used in websocket
        public ActionResult CheckState([FromBody]CheckParams param)
        {
            if (_lifecycleService.State == param.State && _lifecycleService.CurrentTurnId == param.TurnId)
                return StatusCode(304);

            var turn = _turnManager.GetCurrentTurn();
            return Ok(new { CurrentTurn = turn });
        }

        /// Get history or new turn
        public ActionResult GetTurn([FromBody]TurnParams param)
        {
            var turn = _turnManager.GetTurn(param.Id);
            return Ok(turn);
        }

        /// User marks himself as finished
        public ActionResult TurnDone([FromBody]DoneParams param)
        {
            _doneService.Done(_context.CurrentUser.Id, param.Done);

            return Ok();
        }

        // create / update new order 
        public ActionResult SetOrder([FromBody]OrderParams param)
        {
            _turnManager.SetOrder(param, _context.CurrentUser.Id, _lifecycleService.CurrentTurnId);

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