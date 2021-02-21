using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace MySafe.Api.Models
{
    public class RegistrationResponse
    {
        public string Error { get; set; }
        public string Token { get; set; }
    }
}
