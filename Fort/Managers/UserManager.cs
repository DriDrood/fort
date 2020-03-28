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
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<User>();
            _jwtHandler = new JwtHandler();
        }

        private readonly FortDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtHandler _jwtHandler;

        public Login Login(string username, string password)
        {
            // get user
            var user = _dbContext.Users.SingleOrDefault(u => u.UserName == username);
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
                Token = _jwtHandler.GenerateToken(user)
            };
            return login;
        }

        public Dictionary<Guid, Player> GetAllPlayers()
        {
            throw new NotImplementedException();
        }
        public Dictionary<Guid, Team> GetAllTeams()
        {
            throw new NotImplementedException();
        }
    }
}