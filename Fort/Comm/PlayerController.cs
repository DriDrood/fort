using Fort.Managers;
using Fort.Models;
using Fort.Models.Params;
using Fort.Models.Store;
using Fort.Services;

namespace Fort.Comm
{
  public class PlayerController
  {
    public PlayerController(JwtUser jwtUser, DoneService doneService, LifecycleService lifecycleService, MapManager mapManager, TurnManager turnManager, UserManager userManager)
    {
      _jwtUser = jwtUser;
      _doneService = doneService;
      _lifecycleService = lifecycleService;
      _mapManager = mapManager;
      _turnManager = turnManager;
      _userManager = userManager;
    }

    private readonly JwtUser _jwtUser;
    private readonly DoneService _doneService;
    private readonly LifecycleService _lifecycleService;
    private readonly MapManager _mapManager;
    private readonly TurnManager _turnManager;
    private readonly UserManager _userManager;

    /// Login and get initial state
    public Init Login(LoginParams param)
    {
      // authentication
      var login = _userManager.Login(param.Email, param.Password);

      // init
      var init = GetInitData();
      init.Login = login;
      return init;
    }

    /// Get Initial state
    public Init Init()
    {
      return GetInitData();
    }

    /// Get history or new turn
    public Turn GetTurn(TurnParams param)
    {
      return _turnManager.GetTurn(param.Id);
    }

    /// User marks himself as finished
    public SetTurnClosedParams SetTurnClosed(SetTurnClosedParams param)
    {
      _doneService.Done(_jwtUser.UserId, param.Closed);

      return param;
    }

    // create / update new order 
    public void SetOrder(OrderParams param)
    {
      _turnManager.SetOrder(param, _jwtUser.UserId, _lifecycleService.CurrentTurnId);
    }

    private Init GetInitData()
    {
      var initData = new Init();
      initData.CurrentTurn = _turnManager.GetCurrentTurn();
      initData.Turns = new Turn[] { _turnManager.GetTurn(initData.CurrentTurn.Id) };
      initData.Cities = _mapManager.GetAllCities();
      initData.Roads = _mapManager.GetAllRoads();
      initData.Players = _userManager.GetAllPlayers();
      initData.Teams = _userManager.GetAllTeams();
      initData.Config = ConfigManager.Config;

      return initData;
    }
  }
}