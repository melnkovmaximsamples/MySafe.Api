using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MySafe.Api.Models;
using MySafe.Core;
using MySafe.Data.Entities;
using MySafe.Data.Identity;

namespace MySafe.Api.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IMapper _mapper;

        public IdentityService(ApplicationUserManager userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<AuthenticateResponse> AuthenticateAsync(UserRequest request, string ipAddress)
        {
            var applicationUser = await _userManager.FindByNameAsync(request.Username);

            if (applicationUser == null)
            {
                return new AuthenticateResponse() { Errors = new[] { "User not found" } };
            }

            var isCorrectPwd = await _userManager.CheckPasswordAsync(applicationUser, request.Password);

            if (!isCorrectPwd)
            {
                return new AuthenticateResponse() { Errors = new[] { "Password invalid" } };
            }

            var accessToken = GetNewAccessToken(applicationUser, ipAddress, out var jwtAccessToken);
            applicationUser.AccessTokens.Add(accessToken);

            var activeRefreshToken = applicationUser.RefreshTokens.FirstOrDefault(x => x.IsActive);
            var jwtRefreshToken = activeRefreshToken?.JwtToken;

            if (jwtRefreshToken == null)
            {
                activeRefreshToken = GetNewRefreshToken(ipAddress, out jwtRefreshToken);
                applicationUser.RefreshTokens.Add(activeRefreshToken);
            }

            await _userManager.UpdateAsync(applicationUser);

            return new AuthenticateResponse() { AccessToken = jwtAccessToken, RefreshToken = jwtRefreshToken };
        }

        public async Task<AuthenticateResponse> RegisterAsync(UserRequest request, string ipAddress)
        {
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user != null)
            {
                return new AuthenticateResponse() { Errors = new[] { "User already registered" } };
            }

            var applicationUser = _mapper.Map<ApplicationUser>(request);
            var accessToken = GetNewAccessToken(applicationUser, ipAddress, out var jwtAccessToken);
            var refreshToken = GetNewRefreshToken(ipAddress, out var jwtRefreshToken);

            applicationUser.AccessTokens.Add(accessToken);
            applicationUser.RefreshTokens.Add(refreshToken);

            var result = await _userManager.CreateAsync(applicationUser, request.Password);

            if (!result.Succeeded)
            {
                return new AuthenticateResponse() { Errors = result.Errors.Select(x => x.Description).ToArray() };
            }

            return new AuthenticateResponse() { AccessToken = jwtAccessToken, RefreshToken = jwtRefreshToken };
        }

        private ApplicationToken GetNewRefreshToken(string ipAddress, out string refreshToken)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];

            rngCryptoServiceProvider.GetBytes(randomBytes);
            refreshToken = Convert.ToBase64String(randomBytes);

            var applicationToken = new ApplicationToken()
            {
                JwtToken = refreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            return applicationToken;
        }

        public ApplicationToken GetNewAccessToken(ApplicationUser user, string ipAddress, out string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = AppData.AuthOptions.ISSUER,
                Audience = AppData.AuthOptions.AUDIENCE,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(AppData.AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            accessToken = tokenHandler.WriteToken(token);

            var applicationToken = _mapper.Map<ApplicationToken>(token);
            applicationToken.CreatedByIp = ipAddress;

            return applicationToken;
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
