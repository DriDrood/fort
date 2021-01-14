using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using Fort.Database.Entities;
using Fort.Managers;
using Fort.Models;
using Fort.Models.Config;
using Microsoft.IdentityModel.Tokens;

namespace RingoRegistration.backend.Utils
{
  public class JwtHandler
  {
    public string GenerateToken(User user)
    {
      var key = new SymmetricSecurityKey(Convert.FromBase64String(Config.PrivateKey));
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Issuer = null,
        Audience = null,
        IssuedAt = DateTime.UtcNow,
        NotBefore = DateTime.UtcNow,
        Expires = DateTime.UtcNow.AddDays(1),
        Subject = new ClaimsIdentity(new List<Claim> {
          new Claim(ClaimTypes.Email, user.Email),
          new Claim(ClaimTypes.GivenName, user.UserName),
          new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
          new Claim("UserId", user.Id.ToString()),
          new Claim("TeamId", user.TeamId.ToString()),
        }),
        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
      };
      var jwtTokenHandler = new JwtSecurityTokenHandler();
      var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
      var token = jwtTokenHandler.WriteToken(jwtToken);

      return token;
    }

    public void ValidateUpdateJwt(string token, JwtUser jwtUser)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      try
      {
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Config.PrivateKey)),
          ValidateIssuer = false,
          ValidateAudience = false,
          // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);
        var jwtToken = (JwtSecurityToken)validatedToken;

        // update
        jwtUser.Token = token;
        jwtUser.Email = jwtToken.Claims.First(c => c.Type == "email").Value;
        jwtUser.Name = jwtToken.Claims.First(c => c.Type == "given_name").Value;
        jwtUser.Role = jwtToken.Claims.First(c => c.Type == "role").Value;
        jwtUser.UserId = Guid.Parse(jwtToken.Claims.First(c => c.Type == "UserId").Value);
        jwtUser.TeamId = Guid.Parse(jwtToken.Claims.First(c => c.Type == "TeamId").Value);
      }
      catch (Exception ex)
      {
        throw new AuthenticationException(ex.Message);
      }
    }

    private JwtTokenConfig Config => ConfigManager.JwtToken;
  }
}