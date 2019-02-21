using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Fort.Utils
{
    public class RedisService
    {
        public RedisService()
        {
            _config = new Config();
        }
        
        private Config _config;
        private ConnectionMultiplexer _redis;
        private IDatabase _db;
        private IServer _server;

        public void Init(IConfigurationSection configSection)
        {
            configSection.Bind(_config);

            _redis = ConnectionMultiplexer.Connect(_config.Url);
            _db = _redis.GetDatabase();
            _server = _redis.GetServer(_config.Host, 6379);
        }

        public List<string> ListGuids(string prefix)
        {
            int offset = prefix.Length + 1;
            var matchingKeys = _server.Keys(pattern: $"{prefix}:*");
            return matchingKeys.Select(x => x.ToString().Substring(offset)).ToList();
        }

        public Task SetAsync(string key, string value, int expire = 0)
        {
            if (expire > 0)
                return _db.StringSetAsync(key, value, TimeSpan.FromSeconds(expire));
                
            return _db.StringSetAsync(key, value);
        }
        public Task<RedisValue> GetAsync(string key)
        {
            return _db.StringGetAsync(key);
        }

        public Task<bool> AddToSetAsync(string key, string value)
        {
            return _db.SetAddAsync(key, value);
        }
        public Task<bool> RemoveFromSetAsync(string key, string value)
        {
            return _db.SetRemoveAsync(key, value);
        }
        public Task<bool> IsMemberSetAsync(string key, string value)
        {
            return _db.SetContainsAsync(key, value);
        }
        public async Task<IEnumerable<string>> GetAllSetAsync(string key)
        {
            var result = await _db.SetMembersAsync(key);
            return result.Select(i => i.ToString());
        }
        public async Task ClearSetAsync(string key)
        {
            var tasks = new List<Task>();
            foreach (var member in await _db.SetMembersAsync(key))
            {
                tasks.Add(_db.SetRemoveAsync(key, member));
            }

            await Task.WhenAll(tasks);
        }

        private class Config
        {
            public string Host { get; set; }
            public string Url { get; set; }
        }
    }
}