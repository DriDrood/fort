using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fort.Utils.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.Module.Comm
{
    public class WebSocketChannel : IChannel
    {
        public WebSocketChannel(string playerId, WebSocket webSocket)
        {
            PlayerId = playerId;
            _webSocket = webSocket;
            WaitingLoop = receiveLoop();
        }

        public string PlayerId { get; private set; }
        public bool ReadyToSend => _webSocket.State == WebSocketState.Open;
        public CommService Comm { get; set; }
        public Queue Q { get; set; }
        public Task WaitingLoop { get; private set; }
        private const int _bufferSize = 4096;
        private WebSocket _webSocket;

        public async Task SendMessage(string method, object data)
        {
            // create message
            object message = new { method = method, @param = data };
            string messageString = JsonConvert.SerializeObject(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageString);

            // log
            Logger.Log(ELogLevel.MessageSend, PlayerId, messageString);

            // split & send
            try
            {
                while (messageBytes.Length > 0)
                {
                    byte[] buffer = messageBytes.Take(_bufferSize).ToArray();
                    _webSocket.SendAsync(buffer, WebSocketMessageType.Text, messageBytes.Length < _bufferSize, CancellationToken.None).GetAwaiter().GetResult();

                    messageBytes = messageBytes.Skip(_bufferSize).ToArray();
                }
            }
            catch (WebSocketException ex)
            {
                Logger.Log(ELogLevel.UnknownException, PlayerId, ex.Message, ex.StackTrace);
            }
        }

        public void Disconnect(string reason)
        {
            try
            {
                _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (WebSocketException)
            {
                _webSocket.Abort();
            }

            Comm.OnDisconnect(PlayerId, reason);
        }

        private Task receiveLoop()
        {
            return Task.Run(() =>
            {
                try
                {
                    // wait for messages
                    var buffer = new byte[_bufferSize];
                    WebSocketReceiveResult result;

                    do
                    {
                        StringBuilder message = new StringBuilder();
                        do
                        {
                            result = _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).GetAwaiter().GetResult();
                            string messagePart = Encoding.UTF8.GetString(buffer.Take(result.Count).ToArray());
                            message.Append(messagePart);
                        }
                        while (!result.EndOfMessage);

                        // ignore closing
                        if (!result.CloseStatus.HasValue)
                        {
                            JToken messageJ = JToken.Parse(message.ToString());
                            Comm.OnMessage(PlayerId, messageJ["method"].Value<string>(), messageJ["param"]);
                        }
                    }
                    while (!result.CloseStatus.HasValue);

                    Comm.OnDisconnect(PlayerId, result.CloseStatusDescription);
                }
                catch (WebSocketException ex)
                {
                    Comm.OnDisconnect(PlayerId, ex.Message);
                }
            });
        }
    }
}