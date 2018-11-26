using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fort.Services
{
    public class CommService
    {
        public CommService()
        {
            _playerConnections = new Dictionary<string, WebSocket>();
        }

        private Dictionary<string, WebSocket> _playerConnections;
        private int _bufferSize = 4096;

        public async Task CreateNewConnection(string playerId, WebSocket webSocket)
        {
            _playerConnections.Add(playerId, webSocket);

            // wait for messages
            var buffer = new byte[_bufferSize];
            WebSocketReceiveResult result;
            do
            {
                StringBuilder message = new StringBuilder();
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string messagePart = Encoding.UTF8.GetString(buffer);
                    message.Append(messagePart);
                }
                while (!result.EndOfMessage);

                await recieveMessage(playerId, JToken.Parse(message.ToString()));
            }
            while (!result.CloseStatus.HasValue);

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _playerConnections.Remove(playerId);
        }

        private async Task recieveMessage(string playerId, JToken message)
        {
#warning TODO
        }

        public async Task SendToAll(string method, object data)
        {
            List<Task> tasks = new List<Task>();
            foreach (var pair in _playerConnections)
            {
                tasks.Add(send(pair.Value, method, data));
            }

            await Task.Run(() => Task.WaitAll(tasks.ToArray()));
        }

        public async Task SendToOne(string playerId, string method, object data)
        {
            await send(_playerConnections[playerId], method, data);
        }

        private async Task send(WebSocket webSocket, string method, object data)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

            while (messageBytes.Length > 0)
            {
                byte[] buffer = messageBytes.Take(_bufferSize).ToArray();
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, messageBytes.Length < _bufferSize, CancellationToken.None);

                messageBytes = messageBytes.Skip(_bufferSize).ToArray();
            }
        }
    }
}