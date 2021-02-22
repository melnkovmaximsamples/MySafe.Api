using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MySafe.Api.Models;
using MySafe.Core;
using MySafe.Data.Identity;

namespace MySafe.Api.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly ApplicationUserManager _userManager;

        public IdentityService(ApplicationUserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task<AuthenticationResult> CreateAccessTokenAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return new AuthenticationResult() { Errors = new[] { "User not found" } };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = AppData.AuthOptions.ISSUER,
                Audience = AppData.AuthOptions.AUDIENCE
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtAccessToken = tokenHandler.WriteToken(token);
            
            user.RefreshToken = jwtAccessToken;
            await _userManager.UpdateAsync(user);

            return new AuthenticationResult() { AccessToken = jwtAccessToken };
        }

        public async Task<AuthenticationResult> CreateRefreshTokenAsync(string jwtAccessToken)
        {
            var accessToken = new JwtSecurityToken(jwtAccessToken);
            
            var userId = new JwtSecurityToken(jwtAccessToken).Claims
                .Select(x => x.Subject)
                .FirstOrDefault();

            var user = await _userManager.FindByIdAsync(userId?.Name);

            if (user == null)
            {
                return new AuthenticationResult() { Errors = new[] { "User not found" } };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = AppData.AuthOptions.ISSUER,
                Audience = AppData.AuthOptions.AUDIENCE
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtRefreshToken = tokenHandler.WriteToken(token);

            user.RefreshToken = jwtRefreshToken;
            await _userManager.UpdateAsync(user);

            return new AuthenticationResult() { RefreshToken = jwtRefreshToken };
        }

        private bool ValidateToken(JwtSecurityToken token)
        {
            if (token.SignatureAlgorithm != SecurityAlgorithms.HmacSha256)
            {
                return false;
            }

            if (token.Issuer != AppData.AuthOptions.ISSUER)
            {
                return false;
            }

            if (token.Audiences.All(x => x != AppData.AuthOptions.AUDIENCE))
            {
                return false;
            }

            if (token.ValidTo < DateTime.UtcNow.AddMinutes(5))
            {
                return false;
            }


            return true;
        }
    }
}
