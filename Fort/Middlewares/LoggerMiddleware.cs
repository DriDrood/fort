using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fort.Utils.Logger;
using Microsoft.AspNetCore.Http;

namespace Fort.Middlewares
{
    public class LoggerMiddleware
    {
        public LoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private RequestDelegate _next;

        public async Task Invoke(HttpContext context, Logger logger)
        {
            // init
            Guid reqId = Guid.NewGuid();

            // request
            logger.LogRequest(reqId, context.User?.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email)?.Value, context.Request);

            // response
            using (var responseBody = new MemoryStream())
            {
                var originalResponseBody = context.Response.Body;
                context.Response.Body = responseBody;
                
                // run
                try
                {
                    await _next.Invoke(context);
                }
                // catch (RRException ex)
                // {
                //     logger.LogResponse(reqId, 500, ex.Message);
                //     throw;
                // }
                catch (Exception ex)
                {
                    logger.LogException(ex);
                    throw;
                }

                // log response
                logger.LogResponse(reqId, context.Response.StatusCode, context.Response);
                await responseBody.CopyToAsync(originalResponseBody);
            }
        }
    }
}