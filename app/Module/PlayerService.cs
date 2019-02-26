using System;
using System.Threading.Tasks;
using Fort.Database.Entities;
using Fort.Utils;

namespace Fort.Module
{
    public class PlayerService
    {
        public PlayerService(RedisService redis)
        {
            _redis = redis;
        }

        private RedisService _redis;

        public void Login()
        {
        }

        public async Task PlayerReady(ContextService context, int roundId, bool setReady)
        {
            string playerId = context.CurrentPlayer.Id;
            if (setReady)
                await _redis.AddToSetAsync($"fort:playerReady:{roundId}", playerId);
            else
                await _redis.RemoveFromSetAsync($"fort:playerReady:{roundId}", playerId);
        }
        public Task<bool> IsPlayerReady(string playerId, int roundId)
        {
            return _redis.IsMemberSetAsync($"fort:playerReady:{roundId}", playerId);
        }
        public Task CreateUser(ContextService context, string name, string teamId, string imageUrl)
        {
            context.Database.Users.Add(new User
            {
                Id = newPlayerId(),
                Name = name,
                TeamId = teamId,
                IsAdmin = false,
                ImageUrl = imageUrl
            });
            return context.Database.SaveChangesAsync();
        }
        public Task CreateTeam(ContextService context, string name, string color)
        {
            context.Database.Teams.Add(new Team
            {
                Id = newPlayerId(),
                Name = name,
                Color = color
            });
            return context.Database.SaveChangesAsync();
        }

        private string newPlayerId()
        {
            Random rand = new Random();
            char[] guid = new char[5];
            for (int i = 0; i < 5; i++)
            {
                var chI = rand.Next() % 36;
                // char
                if (chI < 26)
                    guid[i] = (char)(chI + 97);
                else
                    guid[i] = (char)(chI - 26 + 48);
            }

            return new string(guid);
        }
    }
}