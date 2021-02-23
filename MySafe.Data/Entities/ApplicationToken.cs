using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MySafe.Data.Entities
{
    public class ApplicationToken
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }
        
        public string JwtToken { get; set; }
        public string CreatedByIp { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }

        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
