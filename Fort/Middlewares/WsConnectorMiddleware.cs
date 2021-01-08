using System;
using System.Threading.Tasks;
using Fort.Extensions;
using Fort.Services;
using Fort.Utils.WebSocket;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fort.Middlewares
{
  public class WsConnectorMiddleware
  {
    public WsConnectorMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    private RequestDelegate _next;

    public Task Invoke(HttpContext context, IServiceProvider serviceProvider, WsConnection wsc)
    {
      wsc.ReceiveMessage = msg => CreateNewScope(serviceProvider, msg);
      // is WebSocket
      if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
        return wsc.Connect(context);
        
      // fail
      context.Response.StatusCode = 400;
      return Task.CompletedTask;
    }

    private void CreateNewScope(IServiceProvider serviceProvider, string message)
    {
      Task.Run(async () =>
      {
        using (var scope = serviceProvider.CreateMessageScope())
        {
          try
          {
            // copy message
            var context = scope.ServiceProvider.GetService<MessageContext>();
            context.InputMessage = message;

            // next
            await _next.Invoke(new DefaultHttpContext() { RequestServices = scope.ServiceProvider });

            // response
            if (context.Response != null)
            {
              
            }
          }
          catch (Exception)
          {
#warning TODO: log error
          }
        }
      });
    }
  }
}