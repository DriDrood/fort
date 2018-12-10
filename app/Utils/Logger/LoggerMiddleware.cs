using System;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Services;
using Microsoft.AspNetCore.Http;

namespace Fort.Utils.Logger
{
    public class LoggerMiddleware
    {
        public LoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private RequestDelegate _next;

        public async Task Invoke(HttpContext context, CurrentPlayerService currentPlayer)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (FortException ex)
            {
                Logger.Log(ex.LogLevel, currentPlayer.Player.Id, ex.Message, ex.StackTrace);
            }
            catch (Exception ex)
            {
                Logger.Log(ELogLevel.UnknownException, currentPlayer.Player.Id, ex.Message, ex.StackTrace);
            }
        }
    }
}