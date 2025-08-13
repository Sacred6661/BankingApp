using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AccountService.IntegrationTests
{
    public static class FakeTokenHelper
    {
        private static RsaSecurityKey? _rsaKey;
        private static SigningCredentials? _signingCredentials;
        private static string? _kid;

        private static string? _issuer;
        private static string? _audience;

        public static void Init(IConfiguration configuration)
        {
            // Читаємо значення з конфіга
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];

            // Генеруємо RSA ключі
            using var rsa = RSA.Create(2048);
            _rsaKey = new RsaSecurityKey(rsa.ExportParameters(true))
            {
                KeyId = Guid.NewGuid().ToString("N")
            };
            _kid = _rsaKey.KeyId;
            _signingCredentials = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
        }

        public static string GenerateToken(string userId, string role, TimeSpan? lifetime = null)
        {
            if (_issuer is null || _audience is null || _signingCredentials is null)
                throw new InvalidOperationException("FakeTokenHelper не ініціалізовано. Викличте Init() перед використанням.");

            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("user_id", userId),
                new Claim(ClaimTypes.Role, role)
            };

            var jwt = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(lifetime ?? TimeSpan.FromHours(1)),
                signingCredentials: _signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public static RsaSecurityKey GetPublicKey() => _rsaKey!;
        public static string GetKid() => _kid!;
    }
}
