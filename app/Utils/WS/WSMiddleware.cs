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

        public async Task Invoke(HttpContext context, CommService commService, ContextService contextService, RoundService roundService)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var channel = new WebSocketChannel(contextService.CurrentPlayer.Id, await context.WebSockets.AcceptWebSocketAsync());
                await commService.CreateConnection(contextService.CurrentPlayer, channel, roundService);
                await channel.WaitingLoop;
            }
            else
                await _next.Invoke(context);
        }
    }
}