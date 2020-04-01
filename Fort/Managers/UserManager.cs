using System;
using System.Collections.Generic;
using System.Linq;
using Fort.Database;
using Fort.Models.Store;
using Microsoft.AspNetCore.Identity;
using RingoRegistration.backend.Utils;
using User = Fort.Database.Entities.User;

namespace Fort.Managers
{
    public class UserManager
    {
        public UserManager(FortDbContext dbContext)
        {
            _db = dbContext;
            _passwordHasher = new PasswordHasher<User>();
            _jwtHandler = new JwtHandler();
        }

        private readonly FortDbContext _db;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtHandler _jwtHandler;

        public Login Login(string username, string password)
        {
            // get user
            var user = _db.Users.SingleOrDefault(u => u.UserName == username);
            if (user == null)
                return null;

            // verify password
            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                user.AccessFailedCount++;
                return null;
            }

            var login = new Login
            {
                Id = user.Id,
                Name = user.UserName,
                JwtToken = _jwtHandler.GenerateToken(user)
            };
            return login;
        }

        public User CreateUser(string username, string password, Guid teamId)
        {
            var user = new User
            {
                UserName = username,
                TeamId = teamId
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _db.Add(user);
            _db.SaveChanges();

            return user;
        }

        public Dictionary<Guid, Player> GetAllPlayers()
        {
            var players = _db.Users.ToDictionary(
                u => u.Id,
                u => new Player
                {
                    Name = u.UserName,
                    TeamId = u.TeamId
                });
            
            return players;
        }
        public Dictionary<Guid, Team> GetAllTeams()
        {
            var teams = _db.Teams.ToDictionary(
                t => t.Id,
                t => new Team
                {
                    Color = t.Color,
                    LightColor = t.ColorLight
                });

            return teams;
        }
    }
}