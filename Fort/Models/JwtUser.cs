using System;
using Fort.Database.Entities;

namespace Fort.Models
{
  public class JwtUser
  {
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public Guid TeamId { get; set; }

    public bool IsAnnonymous => string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Role);

    public void Load(User user, string token)
    {
      Token = token;
      UserId = user.Id;
      Email = user.Email;
      Name = user.UserName;
      Role = user.IsAdmin ? "Admin" : "User";
      TeamId = user.TeamId;
    }
  }
}