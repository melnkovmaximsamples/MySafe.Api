using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySafe.Api.Extensions;
using MySafe.Api.Models;
using MySafe.Api.Services;
using System.Threading.Tasks;

namespace MySafe.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public UsersController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrorResponse());

            var ipAddress = HttpContext.GetIpAddress();
            var result = await _identityService.RegisterAsync(request, ipAddress);

            return result.Errors?.Length > 0 ? BadRequest(result) : Ok(result) as IActionResult;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("sign_in")]
        public async Task<IActionResult> SignIn([FromBody] UserRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrorResponse());
            
            var ipAddress = HttpContext.GetIpAddress();
            var result = await _identityService.AuthenticateAsync(request, ipAddress);
            
            return result.Errors?.Length > 0 ? BadRequest(result) : Ok(result) as IActionResult;
        }
    }
}
