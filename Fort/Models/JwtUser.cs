using System;
using Fort.Database.Entities;

namespace Fort.Models
{
  public class JwtUser
  {
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Given_Name { get; set; }
    public string Role { get; set; }
    public Guid TeamId { get; set; }
    public DateTime NotValidBefore { get; set; }
    public DateTime ExpirationTime { get; set; }
    public DateTime IssuedAt { get; set; }

    public bool IsAnnonymous => string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Given_Name) || string.IsNullOrEmpty(Role);

    public long nbf
    {
      get
      {
        return (long)(NotValidBefore - new DateTime(1970, 1, 1)).TotalSeconds;
      }
      set
      {
        NotValidBefore = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(value);
      }
    }
    public long exp
    {
      get
      {
        return (long)(ExpirationTime - new DateTime(1970, 1, 1)).TotalSeconds;
      }
      set
      {
        ExpirationTime = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(value);
      }
    }
    public long iat
    {
      get
      {
        return (long)(IssuedAt - new DateTime(1970, 1, 1)).TotalSeconds;
      }
      set
      {
        IssuedAt = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(value);
      }
    }

    public void Load(User user, string token)
    {
      Token = token;
      UserId = user.Id;
      Email = user.Email;
      Given_Name = user.UserName;
      Role = user.IsAdmin ? "Admin" : "User";
      TeamId = user.TeamId;
    }
  }
}