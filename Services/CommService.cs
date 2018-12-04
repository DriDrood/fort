using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils.Logger;
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

        private int _bufferSize = 4096;
        private ActionService _actionService => Program.GetService<ActionService>();
        private Dictionary<string, WebSocket> _playerConnections;

        public async Task CreateNewConnection(string playerId, WebSocket webSocket)
        {
            _playerConnections.Add(playerId, webSocket);

            Logger.Log(ELogLevel.Connection, playerId, "User connected");

            // wait for messages
            var buffer = new byte[_bufferSize];
            WebSocketReceiveResult result;
            do
            {
                StringBuilder message = new StringBuilder();
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string messagePart = Encoding.UTF8.GetString(buffer.Take(result.Count).ToArray());
                    message.Append(messagePart);
                }
                while (!result.EndOfMessage);

                // ignore closing
                if (!result.CloseStatus.HasValue)
                    await recieveMessage(playerId, message.ToString());
            }
            while (!result.CloseStatus.HasValue);

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _playerConnections.Remove(playerId);

            Logger.Log(ELogLevel.Connection, playerId, "User disconnected");
        }

        private async Task recieveMessage(string playerId, string message)
        {
            Logger.Log(ELogLevel.MessageReceive, playerId, message);
            try
            {
                JToken data = JToken.Parse(message);

                using (FortDbContext context = new FortDbContext())
                {
                    User user = await Task.Run(() => context.Users.Find(playerId));

                    switch (data["method"].Value<string>().ToLower())
                    {
                        case "turn":
                            Turn playerTurn = data["params"].ToObject<Turn>();
                            _actionService.Turn(user, playerTurn);
                            break;

                        case "play":
                            _actionService.Play(user);
                            break;

                        case "pause":
                            await _actionService.Pause(user);
                            break;

                        case "resume":
                            await _actionService.Resume(user);
                            break;

                        case "end":
                            _actionService.End(user);
                            break;

                        default:
                            throw new FortException(ELogLevel.Warning, "Neznámá funkce");
                    }
                }
            }
            catch (FortException ex)
            {
                Logger.Log(ex.LogLevel, playerId, ex.Message, ex.StackTrace);
            }
            catch (Exception ex)
            {
                Logger.Log(ELogLevel.UnknownException, playerId, ex.Message, ex.StackTrace);
            }
        }

        public async Task SendToAll(string method, object data)
        {
            List<Task> tasks = new List<Task>();
            foreach (var pair in _playerConnections)
            {
                tasks.Add(send(pair.Value, method, data, pair.Key));
            }

            await Task.Run(() => Task.WaitAll(tasks.ToArray()));
        }
        public async Task SendToEach(string method, Func<string, string> getData)
        {
            List<Task> tasks = new List<Task>();
            foreach (var pair in _playerConnections)
            {
                tasks.Add(send(pair.Value, method, getData(pair.Key), pair.Key));
            }

            await Task.Run(() => Task.WaitAll(tasks.ToArray()));
        }

        public async Task SendToOne(string playerId, string method, object data)
        {
            await send(_playerConnections[playerId], method, data, playerId);
        }

        private async Task send(WebSocket webSocket, string method, object data, string playerId)
        {
            // create message
            object message = new { method = method, @params = data };
            string messageString = JsonConvert.SerializeObject(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageString);

            // log
            Logger.Log(ELogLevel.MessageSend, playerId, messageString);

            // split & send
            while (messageBytes.Length > 0)
            {
                byte[] buffer = messageBytes.Take(_bufferSize).ToArray();
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, messageBytes.Length < _bufferSize, CancellationToken.None);

                messageBytes = messageBytes.Skip(_bufferSize).ToArray();
            }
        }
    }
}