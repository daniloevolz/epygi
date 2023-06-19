
using System;
using System.Security.Cryptography;

namespace WebApplicationWecomEpygi.Models
{
    public interface IJwtConfig
    {
        string SecretKey { get; set; }
        string Issuer { get; set; }
    }

    public class JwtConfig : IJwtConfig
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
    }
}

