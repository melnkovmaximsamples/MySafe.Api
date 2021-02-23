using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySafe.Api.Models;

namespace MySafe.Api.Services
{
    public interface IIdentityService
    {
        Task<AuthenticateResponse> RegisterAsync(UserRequest request, string ipAddress);
        Task<AuthenticateResponse> AuthenticateAsync(UserRequest request, string ipAddress);
    }
}
