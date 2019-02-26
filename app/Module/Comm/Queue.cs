using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Utils;

namespace Fort.Module.Comm
{
    public class Queue
    {
        public Queue(string playerId, CommService comm, RedisService redis)
        {
            _comm = comm;
            _redis = redis;
            _playerId = playerId;
        }

        private RedisService _redis;
        private CommService _comm;
        private string _playerId;

        public Task AddItem(QueueItem item)
        {
            return _redis.AddToSetAsync($"Fort:queue:{_playerId}:{item.Lifetime}", $"{item.CreatedAt}:::{item.Data}");
        }

        public async Task<IEnumerable<QueueItem>> GetQueue()
        {
            List<QueueItem> result = new List<QueueItem>();
            var tasks = new List<Task<IEnumerable<QueueItem>>>();
            foreach (Lifetime lifetime in Enum.GetValues(typeof(Lifetime)))
                tasks.Add(getSingleQueue(lifetime));

            foreach (Task<IEnumerable<QueueItem>> task in tasks)
                result.AddRange(await task);

            return result.OrderBy(qi => qi.CreatedAt);
        }
        private async Task<IEnumerable<QueueItem>> getSingleQueue(Lifetime lifetime)
        {
            var items = await _redis.GetAllSetAsync($"Fort:queue:{_playerId}:{lifetime}");
            return items.Select(i =>
            {
                var splitterIndex = i.IndexOf(":::");
                return new QueueItem
                {
                    Data = string.Concat(i.Substring(splitterIndex + ":::".Length)),
                    Lifetime = lifetime,
                    CreatedAt = DateTime.Parse(i.Substring(0, splitterIndex))
                };
            });
        }

        public async Task Reset()
        {
            await _redis.ClearSetAsync($"Fort:queue:{_playerId}:Notification");
            await _redis.ClearSetAsync($"Fort:queue:{_playerId}:DataModification");
        }
    }
}