using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WS = System.Net.WebSockets;

namespace Fort.Utils.WebSocket
{
  public class WsConnection
  {
    public Action<string> ReceiveMessage { get; set; }

    private CancellationTokenSource _listenCancel = new CancellationTokenSource();
    private CancellationTokenSource _sendCancel;
    private WS.WebSocket _webSocket;
    private byte[] _buffer = new byte[1024 * 4];

    public async Task Connect(HttpContext httpContext)
    {
      using (_webSocket = await httpContext.WebSockets.AcceptWebSocketAsync())
      {
        WS.WebSocketReceiveResult result;
        string message = string.Empty;
        do
        {
          do
          {
            result = await _webSocket.ReceiveAsync(_buffer, _listenCancel.Token);
            message += System.Text.Encoding.UTF8.GetString(_buffer, 0, result.Count);
          }
          while (!result.EndOfMessage);

          if (!result.CloseStatus.HasValue)
          {
            ReceiveMessage(message);
            message = string.Empty;
          }
        }
        while (!result.CloseStatus.HasValue);
      }
    }

    public void Disconnect()
    {
      _listenCancel.Cancel();
    }

    public Task Send(string route, object data)
    {
      var dataString = JsonConvert.SerializeObject(new { route, data }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
      var dataByte = System.Text.Encoding.UTF8.GetBytes(dataString);

      _sendCancel = new CancellationTokenSource();
      return _webSocket.SendAsync(dataByte, WS.WebSocketMessageType.Text, true, _sendCancel.Token);
    }

    public void SendCancel()
    {
      _sendCancel.Cancel();
    }
  }
}