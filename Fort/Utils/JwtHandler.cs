using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fort.Database.Entities;
using Fort.Managers;
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
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            var token = jwtTokenHandler.WriteToken(jwtToken);

            return token;
        }

        private JwtTokenConfig Config => ConfigManager.JwtToken;
    }
}