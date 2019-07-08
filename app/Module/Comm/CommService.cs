using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database.Entities;
using Fort.Utils;
using Fort.Utils.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fort.Module.Comm
{
    public class CommService
    {
        public CommService()
        {
            _activeChannels = new Dictionary<string, IChannel>();
            _queues = new Dictionary<string, Queue>();
            _activeContexts = new Dictionary<string, ContextService>();
        }

        private Dictionary<string, IChannel> _activeChannels;
        private Dictionary<string, ContextService> _activeContexts;
        private Dictionary<string, Queue> _queues;

        public void Init(ContextService context, RedisService redis)
        {
            // create Q for each player
            foreach (var user in context.Database.Users)
                _queues.Add(user.Id, new Queue(user.Id, this, redis));
            foreach (var team in context.Database.Teams)
                _queues.Add(team.Id, new Queue(team.Id, this, redis));
        }
        public Task CreateConnection(Player player, IChannel channel, RoundService roundService)
        {
            if (_activeChannels.ContainsKey(player.Id))
            {
                _activeChannels[player.Id].Disconnect("Připojen z jiného zařízení");
            }

            channel.Comm = this;
            _activeChannels.Add(player.Id, channel);
            _activeContexts.Add(player.Id, new ContextService { CurrentPlayer = player });
            return _queues[player.Id].Reset();
        }
        public void Disconnect(string playerId, string reason)
        {
            var channel = _activeChannels[playerId];
            channel.Disconnect(reason);
        }
        public void OnDisconnect(string playerId, string reason)
        {
            if (_activeContexts.ContainsKey(playerId))
            {
                _activeContexts[playerId].Dispose();
                _activeContexts.Remove(playerId);
                _activeChannels.Remove(playerId);
            }

            Logger.Log(ELogLevel.Warning, playerId, reason);
        }

        public Task SendOne(ContextService context, string userId, string method, object data, Lifetime lifetimeInQ)
        {
            var dataString = JsonConvert.SerializeObject(new { method, data });

            if (_activeChannels.ContainsKey(userId) && _activeChannels[userId].ReadyToSend)
                return _activeChannels[userId].SendMessage(method, data);

            return _queues[userId].AddItem(new QueueItem
            {
                Data = dataString,
                Lifetime = lifetimeInQ
            });
        }
        public async Task SendToAdmins(ContextService context, string method, object data, Lifetime lifetimeInQ)
        {
            var users = context.Database.Users.Where(u => u.IsAdmin).ToList();
            var tasks = new List<Task>();

            foreach (var user in users)
                tasks.Add(SendOne(context, user.Id, method, data, lifetimeInQ));

            await Task.WhenAll(tasks);
        }
        public async Task SendAll(ContextService context, string method, object data, Lifetime lifetimeInQ)
        {
            var users = context.Database.Users.ToList();
            var tasks = new List<Task>();

            foreach (var user in users)
                tasks.Add(SendOne(context, user.Id, method, data, lifetimeInQ));

            await Task.WhenAll(tasks);
        }
        public async Task SendEach(ContextService context, string method, Func<ContextService, object> getData, Lifetime lifetimeInQ)
        {
            var users = context.Database.Users.ToList();
            var tasks = new List<Task>();

            foreach (var user in users)
            {
                var data = getData(_activeContexts.ContainsKey(user.Id)
                    ? _activeContexts[user.Id]
                    : new ContextService { CurrentPlayer = user });
                var task = SendOne(context, user.Id, method, data, lifetimeInQ);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        public void OnMessage(string playerId, string method, JToken data)
        {
            var context = _activeContexts[playerId];
            OnMessageEvent.Invoke(context, method, data);
        }
        public event Action<ContextService, string, JToken> OnMessageEvent;
    }
}