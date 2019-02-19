using System;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Module;
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

        public async Task Invoke(HttpContext context, ContextService contextService)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (FortException ex)
            {
                Logger.Log(ex.LogLevel, contextService.CurrentPlayer?.Id, ex.Message, ex.StackTrace);
            }
            catch (Exception ex)
            {
                Logger.Log(ELogLevel.UnknownException, contextService.CurrentPlayer?.Id, ex.Message, ex.StackTrace);
            }
        }
    }
}