using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fort.Models;
using Fort.Services;
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

    public async Task Invoke(HttpContext _, JwtUser jwtUser, MessageContext context, Logger logger)
    {
      // log request
      logger.LogRequest(context.RequestId, jwtUser.Email, context.InputMessage);

      // run
      try
      {
        await _next.Invoke(_);
      }
      catch (Exception ex)
      {
        logger.LogException(ex);
      }
    }
  }
}