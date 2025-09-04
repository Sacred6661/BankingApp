using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;

namespace ProfileService.Helpers
{
    public static class JwtHelper
    {
        public static async Task<IEnumerable<SecurityKey>> FetchSigningKeysFromJwks(string jwksUrl)
        {
            using var http = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

            var json = await http.GetStringAsync(jwksUrl);

            using var doc = JsonDocument.Parse(json);
            var keys = new List<SecurityKey>();

            foreach (var element in doc.RootElement.GetProperty("keys").EnumerateArray())
            {
                var kty = element.GetProperty("kty").GetString();
                var use = element.GetProperty("use").GetString();
                var kid = element.GetProperty("kid").GetString();
                var n = element.GetProperty("n").GetString();
                var e = element.GetProperty("e").GetString();

                if (kty == "RSA" && use == "sig")
                {
                    var rsa = new RSACryptoServiceProvider();
                    rsa.ImportParameters(new RSAParameters
                    {
                        Modulus = Base64UrlDecode(n),
                        Exponent = Base64UrlDecode(e)
                    });

                    keys.Add(new RsaSecurityKey(rsa) { KeyId = kid });
                }
            }

            return keys;
        }

        // Base64Url decoder
        private static byte[] Base64UrlDecode(string input)
        {
            var pad = input.Length % 4;
            if (pad > 0) input += new string('=', 4 - pad);
            input = input.Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(input);
        }
    }
}
