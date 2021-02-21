using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MySafe.Api.Models;
using MySafe.Core;
using MySafe.Data.EF;
using MySafe.Data.Entities;
using MySafe.Data.Identity;

namespace MySafe.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly ApplicationUserManager _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationContext context, ApplicationUserManager userManager, SignInManager<ApplicationUser> signInManager,
            ILogger<UsersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] User userRequest)
        {
            userRequest.Username = "username228";
            userRequest.Email = "melnikovmaxim@icloud.com";
            userRequest.Password = "Ssd17xDldD!";

            if (!ModelState.IsValid)
            {
                return BadRequest(new RegistrationResponse()
                {
                    Error = string.Join(Environment.NewLine, ModelState.Values.Select(x => x.Errors))
                });
            }

            var existsUser = null != await _userManager.FindByNameAsync(userRequest.Username);

            if (existsUser)
            {
                return BadRequest(new RegistrationResponse()
                {
                    Error = "Username already exist"
                });
            }

            var user = new ApplicationUser()
            {
                UserName = userRequest.Username,
                Email = userRequest.Email
            };

            var result = await _userManager.CreateAsync(user, userRequest.Password);

            if (result.Succeeded)
            {
                var jwtToken = GenerateJwtToken(user);

                return Ok(jwtToken);
            }

            return BadRequest(new RegistrationResponse()
            {
                Error = string.Join(Environment.NewLine, result.Errors)
            });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = AppData.AuthOptions.ISSUER,
                Audience = AppData.AuthOptions.AUDIENCE,
                Subject = new ClaimsIdentity(new []
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
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

        [HttpPost]
        [Route("sign_in")]
        public async Task<IActionResult> SignIn([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                // check if the user with the same email exist
                var existingUser = await _userManager.FindByNameAsync(user.Username);

                if(existingUser == null) 
                {
                    // We dont want to give to much information on why the request has failed for security reasons
                    return BadRequest(new RegistrationResponse() {
                        Error = 
                            "Invalid authentication request"

                    });
                }

                // Now we need to check if the user has inputed the right password
                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);

                if(isCorrect)
                {
                    var jwtToken = GenerateJwtToken(existingUser);

                    return Ok(new RegistrationResponse() {
                        Token = jwtToken
                    });
                }
                else 
                {
                    // We dont want to give to much information on why the request has failed for security reasons
                    return BadRequest(new RegistrationResponse() {
                        Error = 
                            "Invalid authentication request"

                    });
                }

                // HttpContext.Response.Headers.Add("Authorization", $"Bearer {encodedJwt}");
            }

            return BadRequest(new RegistrationResponse() {
                Error = 
                    "Invalid payload"
                });
        }

        [HttpPut]
        [Route("two_factor_authentication")]
        public IActionResult TwoFactorAuthentication()
        {
            var token = HttpContext.Request.Headers["Authentication"][0].Replace("Bearer ", "");
            var jwtToken = new JwtSecurityToken(token);

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                issuer: "MySafe.Api",
                audience: "MySafe.Api",
                notBefore: now,
                // claims: identity.Claims
                expires: now.AddMinutes(60),
                signingCredentials: new SigningCredentials(AppData.AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            encodedJwt =
                "eyJhbGciOiJIUzI1NiJ9.eyJqdGkiOiI5NGJjOWIxZi0yZWFjLTRhM2EtYmQ1Ni0wZjMzNTgzNTRhNjUiLCJzZmMiOm51bGwsInN1YiI6IjI1Iiwic2NwIjoidXNlciIsImF1ZCI6bnVsbCwiaWF0IjoxNjEyMzU1NzU0LCJleHAiOjE2MTIzNTkzNTR9.YRc7_xQyPunkvyXp5W5UD2ppkB2PzyMRLViODKrGvPE";
            HttpContext.Response.Headers.Add("Authorization", $"Bearer {encodedJwt}");

            return Ok();
        }

    }
}
