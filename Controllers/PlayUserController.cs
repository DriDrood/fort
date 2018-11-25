using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Fort.Database;
using Fort.Services;

namespace Fort.Controllers
{
    public class PlayUserController : PlayBaseController
    {
        public PlayUserController(FortDbContext context)
        {
            _context = context;
        }

        private FortDbContext _context;

        protected override MapBaseService _mapService => Program.GetService<MapUserService>();
        protected override bool _existThisCode(string code) => _context.Users.Any(u => u.Id == code);
    }
}