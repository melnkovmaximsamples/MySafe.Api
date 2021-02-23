using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MySafe.Api.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetIpAddress(this HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return context.Request.Headers["X-Forwarded-For"];
            }

            return context.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
