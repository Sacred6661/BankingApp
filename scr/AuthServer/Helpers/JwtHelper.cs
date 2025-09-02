using AuthServer.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AuthServer.Helpers
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

        public static Models.JwtPayload GetPayloadData(string accessToken)
        {
            try
            {
                var parts = accessToken.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));

                var result = JsonConvert.DeserializeObject<Models.JwtPayload>(payloadJson);
                return result;
            }
            catch(Exception ex)
            {
                return null;
            }
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
