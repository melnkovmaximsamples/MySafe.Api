﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MySafe.Data.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}