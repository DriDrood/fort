using System;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Module;
using Fort.Utils.Logger;
using Microsoft.AspNetCore.Http;

namespace Fort.Utils
{
    public class CurrentPlayerMiddleware
    {

        public CurrentPlayerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private RequestDelegate _next;

        public async Task Invoke(HttpContext context, ContextService contextService)
        {
            var paths = context.Request.Path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries).Select(p => p.ToLower()).ToList();

            if (paths.Count > 0 && paths[0].Length <= 5)
            {
                contextService.CurrentPlayer = (Player)contextService.Database.Users.Find(paths[0])
                    ?? contextService.Database.Teams.Find(paths[0]);

                if (contextService.CurrentPlayer == null)
                {
                    Logger.Logger.Log(ELogLevel.Warning, paths[0], "Neplatný kód");
                    context.Response.Redirect("/?message=neplatny%20kod", false);
                    return;
                }
            }
            
            await _next.Invoke(context);
        }
    }
}