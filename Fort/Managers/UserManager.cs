using System;
using System.Collections.Generic;
using System.Linq;
using Fort.Database;
using Fort.Models.Store;
using Fort.Services;
using Microsoft.AspNetCore.Identity;
using RingoRegistration.backend.Utils;
using User = Fort.Database.Entities.User;
using System.Security.Authentication;
using Fort.Models;

namespace Fort.Managers
{
  public class UserManager
  {
    public UserManager(JwtUser jwtUser, FortDbContext dbContext)
    {
      _jwtUser = jwtUser;
      _db = dbContext;
      _passwordHasher = new PasswordHasher<User>();
      _jwtHandler = new JwtHandler();
    }

    private readonly JwtUser _jwtUser;
    private readonly FortDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly JwtHandler _jwtHandler;

    public Login Login(string email, string password)
    {
      // get user
      var user = _db.Users.SingleOrDefault(u => u.Email == email)
        ?? throw new AuthenticationException("Invalid email");

      // verify password
      var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
      if (verifyResult == PasswordVerificationResult.Failed)
      {
        user.AccessFailedCount++;
        _db.SaveChanges();
        throw new AuthenticationException("Invalid password");
      }

      // success
      user.AccessFailedCount = 0;
      _db.SaveChanges();
      
      var token = _jwtHandler.GenerateToken(user);
      _jwtUser.Load(user, token);

      var login = new Login
      {
        Id = user.Id,
        Name = user.UserName,
        JwtToken = token,
      };
      return login;
    }

    public User CreateUser(string email, string username, string password, Guid teamId)
    {
      var user = new User
      {
        Email = email,
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
          ColorLight = t.ColorLight
        });

      return teams;
    }
  }
}