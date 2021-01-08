using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Fort.Models;
using Fort.Utils.WebSocket;

namespace Fort.Middlewares
{
  public class WsResponderMiddleware
  {
    public WsResponderMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    private RequestDelegate _next;

    public async Task Invoke(HttpContext _, MessageContext context, WsConnection wsConnection)
    {
      try
      {
        await _next.Invoke(_);

        if (context.Response != null)
          await wsConnection.Send(context.RequestId, context.Route, context.Response);
      }
      catch (Exception ex)
      {
        await wsConnection.Send(context.RequestId, "error", new { message = ex.Message, stackTrace = ex.StackTrace });
        throw;
      }
    }
  }
}