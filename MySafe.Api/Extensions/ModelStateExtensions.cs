using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MySafe.Api.Models;

namespace MySafe.Api.Extensions
{
    public static class ModelStateExtensions
    {
        public static AuthenticateResponse GetErrorResponse(this ModelStateDictionary modelState)
        {
            var errors = modelState.Values
                .SelectMany(x => x.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();

            return new AuthenticateResponse() { Errors = errors };
        }
    }
}
