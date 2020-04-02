using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Services;
using Microsoft.AspNetCore.Http;

namespace Fort.Middlewares
{
    public class ContextMiddleware
    {
        public ContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private RequestDelegate _next;

        public async Task Invoke(HttpContext context, FortDbContext dbContext, Context fortContext)
        {
            var userEmail = context.User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (userEmail != null)
            {
                var user = dbContext.Users.SingleOrDefault(u => u.Email == userEmail);
                fortContext.CurrentUser = user;
            }

            await _next.Invoke(context);
        }
    }
}