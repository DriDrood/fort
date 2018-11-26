using System;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Services;
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

        public async Task Invoke(HttpContext context, FortDbContext dbContext, CurrentPlayerService currentPlayer)
        {
            var paths = context.Request.Path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries).Select(p => p.ToLower()).ToList();

            if (paths.Count > 1 && paths[0] == "admin")
            {
                if (paths.Count > 2 && paths[1] == "mapcreator")
                    currentPlayer.User = dbContext.Users.Find(paths[2]);
                else
                    currentPlayer.User = dbContext.Users.Find(paths[1]);

                // is Admin?
                if (currentPlayer.User?.IsAdmin != true)
                {
                    Logger.Logger.Log(ELogLevel.Warning, currentPlayer.User?.Id, "User not found or is not admin");
                    context.Response.Redirect("/Admin");
                    return;
                }
            }

            else if (paths.Count > 1 && paths[0] == "team")
            {
                currentPlayer.Team = dbContext.Teams.Find(paths[1]);
                if (currentPlayer.Team == null)
                {
                    Logger.Logger.Log(ELogLevel.Warning, paths[1], "Team not found");
                    context.Response.Redirect("/Team");
                    return;
                }
            }

            else if (paths.Count > 0 && paths[0] != "team" && paths[0] != "admin")
            {
                currentPlayer.User = dbContext.Users.Find(paths[0]);
                if (currentPlayer.User == null)
                {
                    Logger.Logger.Log(ELogLevel.Warning, paths[0], "User not found");
                    context.Response.Redirect("/");
                    return;
                }
            }

            await _next.Invoke(context);
        }
    }
}