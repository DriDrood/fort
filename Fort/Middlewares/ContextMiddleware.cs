using System;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Models;
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
            // var paths = context.Request.Path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries).Select(p => p.ToLower()).ToList();

            // if (paths.Count > 0 && paths[0].Length <= 5)
            // {
            //     currentPlayer.Player = (Player)dbContext.Users.Find(paths[0])
            //         ?? dbContext.Teams.Find(paths[0]);

            //     if (currentPlayer.Player == null)
            //     {
            //         Logger.Logger.Log(ELogLevel.Warning, paths[0], "Neplatný kód");
            //         context.Response.Redirect("/?message=neplatny%20kod", false);
            //         return;
            //     }
            // }
            
            await _next.Invoke(context);
        }
    }
}