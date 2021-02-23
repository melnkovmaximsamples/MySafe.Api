using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySafe.Api.Attributes;

namespace MySafe.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")] // api/todo
    [ApiController]
    public class TodoController : ControllerBase
    {
        [HttpGet]
        [Route("[action]")]
        public IActionResult GetString()
        {
            var claims = HttpContext.User.Identity;

            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                var username = identity.FindFirst(ClaimTypes.Name).Value;
                return Content(username);
            }

            return Content("not found");
        }
    }
}
