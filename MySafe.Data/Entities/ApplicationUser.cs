using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace MySafe.Data.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public List<ApplicationToken> AccessTokens { get; set; }
        public List<ApplicationToken> RefreshTokens { get; set; }

        public ApplicationUser()
        {
            AccessTokens = new List<ApplicationToken>();
            RefreshTokens = new List<ApplicationToken>();
        }
    }
}