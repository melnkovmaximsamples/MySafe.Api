using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MySafe.Api.Models
{
    public class AuthenticateResponse
    {
        public string[] Errors { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}
