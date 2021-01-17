using System;
using System.Threading.Tasks;
using Fort.Extensions;
using Fort.Models;
using Fort.Services;
using Fort.Utils.Logger;
using Fort.Utils.WebSocket;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fort.Middlewares
{
  public class WsConnectorMiddleware
  {
    public WsConnectorMiddleware(RequestDelegate next, ConnectionsService connectionsService, Logger logger)
    {
      _next = next;
      _connectionsService = connectionsService;
      _logger = logger;
    }

    private RequestDelegate _next;
    private readonly ConnectionsService _connectionsService;
    private readonly Logger _logger;

    public async Task Invoke(HttpContext context, IServiceProvider serviceProvider, WsConnection wsc)
    {
      // not WebSocket
      if (context.Request.Path != "/ws" || !context.WebSockets.IsWebSocketRequest) {
        context.Response.StatusCode = 400;
        return;
      }
        
      // connect
      _connectionsService.NewConnection(serviceProvider);

      // listen
      wsc.ReceiveMessage = msg => CreateNewScope(serviceProvider, msg);
      await wsc.Connect(context);

      // disconnect
      _connectionsService.Disconnected(serviceProvider);
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
          catch (Exception ex)
          {
            _logger.LogException(ex);
          }
        }
      });
    }
  }
}