using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fort.Utils.Channels;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Utils.Logger;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fort.Services
{
    public class CommService
    {
        public CommService()
        {
            _playerConnections = new Dictionary<string, IChannel>();
        }

        private ActionService _actionService => Program.GetService<ActionService>();
        private Dictionary<string, IChannel> _playerConnections;

        public void CreateNewConnection(IChannel channel)
        {
            // disconnect previous
            if (_playerConnections.ContainsKey(channel.PlayerId))
                _playerConnections[channel.PlayerId].Disconnect("Uživatel se znovu přihlásil");

            channel.OnMessage = recieveMessage;
            channel.OnDisconnect = playerDisconected;
            _playerConnections.Add(channel.PlayerId, channel);

            Logger.Log(ELogLevel.Connection, channel.PlayerId, "User connected");
            SendToAll("playerConnected", new { playerId = channel.PlayerId });
        }


        public bool IsPlayerConnected(string playerId)
        {
            return _playerConnections.ContainsKey(playerId);
        }

        private void recieveMessage(string playerId, string message)
        {
            Logger.Log(ELogLevel.MessageReceive, playerId, message);
            try
            {
                JToken data = JToken.Parse(message);

                using (FortDbContext context = new FortDbContext())
                {
                    User user = context.Users.Include(u => u.Cities).FirstOrDefault(u => u.Id == playerId);

                    switch (data["method"].Value<string>().ToLower())
                    {
                        case "turn":
                            try
                            {
                                Turn playerTurn = data["param"].ToObject<Turn>();
                                _actionService.Turn(user, playerTurn);
                                playerTurn.User = user;
                                playerTurn.SourceCity = context.Cities.Find(playerTurn.SourceCityId);
                                playerTurn.TargetCity = context.Cities.Find(playerTurn.TargetCityId);
                                user.Team = context.Teams.Find(user.TeamId);
                                var order = MapBaseService.GetMapServiceForPlayer(context, user).GetOrder(playerTurn);
                                SendToOne(playerId, "turnOk", new { message = "Tah úspěšně zadán", order = new { x = order.x, y = order.y, color = order.color } });
                            }
                            catch (FortException ex)
                            {
                                SendToOne(playerId, "turnError", ex.Message);
                                Logger.Log(ELogLevel.Warning, playerId, ex.Message, ex.StackTrace);
                            }
                            catch (Exception ex)
                            {
                                SendToOne(playerId, "turnError", ex.Message);
                                throw;
                            }
                            break;

                        case "play":
                            _actionService.Play(user);
                            SendToAll("notification", new { type = "success", message = "Hra spuštěna" });
                            break;

                        case "pause":
                            _actionService.Pause(user);
                            SendToOne(playerId, "notification", new { type = "success", message = "Hra pozastavena" });
                            break;

                        case "end":
                            _actionService.End(user);
                            SendToOne(playerId, "notification", new { type = "success", message = "Kolo ukončeno" });
                            break;

                        case "restart":
                            _actionService.RestartAll(user);
                            SendToAll("notification", new { type = "warning", message = "Restart hry" });
                            break;

                        case "jserror":
                            Logger.Log(ELogLevel.JS, playerId, data["param"]["message"].Value<string>(), $"{data["param"]["url"].Value<string>()} - line {data["param"]["line"].Value<string>()}");
                            break;

                        case "playerready":
                            _actionService.Ready(user, data["param"]["ready"].Value<bool>());
                            break;

                        default:
                            throw new FortException(ELogLevel.Warning, "Neznámá funkce");
                    }
                }
            }
            catch (FortException ex)
            {
                Logger.Log(ex.LogLevel, playerId, ex.Message, ex.StackTrace);
                SendToOne(playerId, "notification", new { type = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                Logger.Log(ELogLevel.UnknownException, playerId, ex.Message, ex.StackTrace);
                SendToOne(playerId, "notification", new { type = "exception", message = ex.Message });
            }
        }
        private void playerDisconected(string playerId, string message)
        {
            _playerConnections.Remove(playerId);

            Logger.Log(ELogLevel.Connection, playerId, $"User disconnected - {message}");
            SendToAll("playerDisconnected", new { playerId = playerId });
        }

        public void SendToAll(string method, object data)
        {
            List<Task> tasks = new List<Task>();
            foreach (var key in _playerConnections.Keys.ToList())
            {
                if (_playerConnections.ContainsKey(key))
                    tasks.Add(Task.Run(() => send(_playerConnections[key], method, data, key)));
            }

            foreach (Task task in tasks)
                task.GetAwaiter().GetResult();
        }
        public void SendToEach(string method, Func<string, string> getData)
        {
            List<Task> tasks = new List<Task>();
            foreach (var key in _playerConnections.Keys.ToList())
            {
                if (_playerConnections.ContainsKey(key))
                {
                    var data = getData(key);
                    tasks.Add(Task.Run(() => send(_playerConnections[key], method, data, key)));
                }
            }

            foreach (Task task in tasks)
                task.GetAwaiter().GetResult();
        }

        public void SendToOne(string playerId, string method, object data)
        {
            if (_playerConnections.ContainsKey(playerId))
                send(_playerConnections[playerId], method, data, playerId);
        }

        private void send(IChannel channel, string method, object data, string playerId)
        {
            channel.SendMessage(method, data);
        }
    }
}