using System.Threading.Tasks;
using Fort.Module;
using Fort.Module.Comm;
using Microsoft.AspNetCore.Http;

namespace Fort.Utils.WS
{
    public class WSMiddleware
    {

        public WSMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private RequestDelegate _next;

        public async Task Invoke(HttpContext context, CommService commService, ContextService contextService)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var channel = new WebSocketChannel(contextService.CurrentPlayer.Id, await context.WebSockets.AcceptWebSocketAsync());
                commService.CreateConnection(contextService.CurrentPlayer, channel);
                await channel.WaitingLoop;
            }
            else
                await _next.Invoke(context);
        }
    }
}