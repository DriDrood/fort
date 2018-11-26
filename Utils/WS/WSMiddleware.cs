using System.Threading.Tasks;
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
                await commService.CreateNewConnection(currentPlayer.ToString(), await context.WebSockets.AcceptWebSocketAsync());
            else
                await _next.Invoke(context);
        }
    }
}