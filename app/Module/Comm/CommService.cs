using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database.Entities;
using Fort.Utils.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace app.Module.Comm
{
    public class CommService
    {
        public CommService(ContextService context)
        {
            _activeChannels = new Dictionary<string, IChannel>();
            _queues = new Dictionary<string, Queue>();

            // create Q for each user
            foreach (var user in context.Database.Users)
                _queues.Add(user.Id, new Queue(this));
            foreach (var team in context.Database.Teams)
                _queues.Add(team.Id, new Queue(this));
        }

        private Dictionary<string, IChannel> _activeChannels;
        private Dictionary<string, Queue> _queues;

        public void CreateConnection(Player player, IChannel channel)
        {
            channel.Comm = this;
            _queues[player.Id].Reset();
            _activeChannels.Add(player.Id, channel);
        }
        public void Disconnect(string playerId, string reason)
        {
            var channel = _activeChannels[playerId];
            channel.Disconnect(reason);
        }
        public void OnDisconnect(string playerId, string reason)
        {
            Logger.Log(ELogLevel.Warning, playerId, reason);
        }

        public Task SendOne(ContextService context, string userId, string method, object data, Lifetime lifetimeInQ)
        {
            var dataString = JsonConvert.SerializeObject(new { method, data });

            if (_activeChannels.ContainsKey(userId) && _activeChannels[userId].ReadyToSend)
                return _activeChannels[userId].SendMessage(method, data);

            _queues[userId].AddItem(new QueueItem
            {
                Data = dataString,
                Lifetime = lifetimeInQ
            });
            return Task.CompletedTask;
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
        public async Task SendEach(ContextService context, string method, Func<User, object> getData, Lifetime lifetimeInQ)
        {
            var users = context.Database.Users.ToList();
            var tasks = new List<Task>();

            foreach (var user in users)
            {
                var data = getData(user);
                var task = SendOne(context, user.Id, method, data, lifetimeInQ);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        public void OnMessage(string playerId, string method, JToken data)
        {

        }
    }
}