using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace MySafe.Core
{
    public partial class AppData
    {
        public class AuthOptions
        {
            public const string ISSUER = "MySafe.Api"; // издатель токена
            public const string AUDIENCE = "MySafe.Clients"; // потребитель токена
            const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
            public const int LIFETIME = 1; // время жизни токена - 1 минута

            public static SymmetricSecurityKey GetSymmetricSecurityKey()
            {
                return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
            }
        }
    }
}
