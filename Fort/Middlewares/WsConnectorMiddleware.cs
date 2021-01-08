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
    public WsConnectorMiddleware(RequestDelegate next, IServiceProvider serviceProvider, WsConnection wsc)
    {
      _next = next;
      _serviceProvider = serviceProvider;
      _wsc = wsc;

      _wsc.ReceiveMessage = CreateNewScope;
    }

    private RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly WsConnection _wsc;

    public Task Invoke(HttpContext context)
    {
      // is WebSocket
      if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
        return _wsc.Connect(context);
        
      // fail
      context.Response.StatusCode = 400;
      return Task.CompletedTask;
    }

    private void CreateNewScope(string message)
    {
      Task.Run(async () =>
      {
        using (var scope = _serviceProvider.CreateMessageScope())
        {
          try
          {
            // copy message
            var context = scope.ServiceProvider.GetService<MessageContext>();
            context.InputMessage = message;

            // next
            await _next.Invoke(new DefaultHttpContext());

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