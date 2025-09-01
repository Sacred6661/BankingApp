using ApiGateway.Models;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace ApiGateway.Middlewares
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public TokenRefreshMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var accessToken = context.Request.Cookies["access_token"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(accessToken);

                var exp = jwt.ValidTo;
                if (exp < DateTime.UtcNow) // token expired
                {
                    // refresg function endpoint
                    var refreshUrl = $"{_config["AuthServerUrl"]}/refresh-token";

                    var refreshRequest = new HttpRequestMessage(HttpMethod.Post, refreshUrl);
                    refreshRequest.Headers.Add("Cookie", context.Request.Headers["Cookie"].ToString());

                    var response = await _httpClient.SendAsync(refreshRequest);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<LoginResponse>(json);

                        if (data != null && data.AccessToken != null)
                        {
                            // add new token to cookies
                            context.Response.Cookies.Append("access_token", data.AccessToken, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddMinutes(10) // залежно від TTL
                            });

                            // change for cyrrent request token too
                            context.Request.Headers["Authorization"] = $"Bearer {data.AccessToken}";
                        }
                    }
                    else
                    {
                        // refresh token is not valid too - redirect too login
                        context.Response.StatusCode = 401;
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
