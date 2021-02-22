using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySafe.Api.Models
{
    public class AuthenticationResult
    {
        public string[] Errors { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
