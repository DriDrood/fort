using System.Linq;
using Fort.Database;
using Fort.Services;

namespace Fort.Controllers
{
    public class PlayTeamController : PlayBaseController
    {
        public PlayTeamController(FortDbContext context, MapTeamService mapTeamService, CurrentPlayerService currentPlayerService)
        {
            _context = context;
            _map = mapTeamService;
            _currentPlayerService = currentPlayerService;
        }

        private FortDbContext _context;
        private MapTeamService _map;
        private CurrentPlayerService _currentPlayerService;

        protected override MapBaseService MapService => _map;
        protected override bool ExistThisCode(string code) => _context.Teams.Any(u => u.Id == code);
        protected override void Set(string code)
        {
            _currentPlayerService.Team = _context.Teams.Find(code);
        }
    }
}