using System.Linq;
using Fort.Database;
using Fort.Services;

namespace Fort.Controllers
{
    public class PlayAdminController : PlayBaseController
    {
        public PlayAdminController(FortDbContext context, MapAdminService mapAdminService, CurrentPlayerService currentPlayerService)
        {
            _context = context;
            _map = mapAdminService;
            _currentPlayerService = currentPlayerService;
        }

        private FortDbContext _context;
        private MapAdminService _map;
        private CurrentPlayerService _currentPlayerService;

        protected override MapBaseService MapService => _map;
        protected override bool ExistThisCode(string code) => _context.Users.Any(u => u.Id == code && u.IsAdmin);
        protected override void Set(string code)
        {
            _currentPlayerService.Team = _context.Teams.Find(code);
        }
    }
}