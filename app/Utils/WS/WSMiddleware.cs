using System.Threading.Tasks;
using Fort.Utils.Channels;
using Fort.Services;
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

        public async Task Invoke(HttpContext context, CommService commService, CurrentPlayerService currentPlayer)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var channel = new WebSocketChannel(currentPlayer.ToString(), await context.WebSockets.AcceptWebSocketAsync());
                commService.CreateNewConnection(channel);
                await channel.WaitingLoop;
            }
            else
                await _next.Invoke(context);
        }
    }
}