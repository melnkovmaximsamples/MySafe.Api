using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MySafe.Data.Entities;

namespace MySafe.Api.Filters
{
    //TODO: maybe to remove, if on tokenvalidation in startup is OK
    public class AuthorizeFilter : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!IsRequiredTokenValidation(context)) return;

            // TODO access_token to constants
            var jwtAccessToken = await context.HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");

            var userManager = context.HttpContext.RequestServices
                .GetService(typeof(UserManager<ApplicationUser>)) as UserManager<ApplicationUser>;

            if (userManager == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var user = await userManager.Users
                .Include(e => e.AccessTokens)
                .FirstOrDefaultAsync(x => x.AccessTokens
                    .Any(t => t.JwtToken == jwtAccessToken));

            var accessToken = user?.AccessTokens.FirstOrDefault(x => x.JwtToken == jwtAccessToken);

            if (accessToken?.IsActive == true) return;

            context.Result = new UnauthorizedResult();

        }

        private bool IsRequiredTokenValidation (AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                var controllerAttributes = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(inherit: true);
                var methodAttributes = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true);

                var methodAllowAnonym = methodAttributes
                    .OfType<AllowAnonymousAttribute>()
                    .Any();

                if (methodAllowAnonym) return false;

                var hasControllerAuhtorizeAttr = controllerAttributes
                    .OfType<AuthorizeAttribute>()
                    .Any();

                var hasMethodAuthorizeAttr = methodAttributes
                    .OfType<AuthorizeAttribute>()
                    .Any();

                return hasControllerAuhtorizeAttr || hasMethodAuthorizeAttr;
            }

            // TODO add logging

            return true;
        }
    }
}
