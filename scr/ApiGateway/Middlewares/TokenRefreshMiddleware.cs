using ApiGateway.Models;
using MassTransit.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Nest;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiGateway.Middlewares
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenRefreshOptions _options;
        private readonly IMemoryCache _cache;
        public TokenRefreshMiddleware(
            RequestDelegate next,
            IHttpClientFactory httpClientFactory,
            IOptions<TokenRefreshOptions> options,
            IMemoryCache cache)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();

            // check for path with only "/", maybe this should be checked in future
            // but now it not working without this one
            // TODO: check if https://address/ will be public in finished system, but looks like yes
            if(path != null && path == "/")
            {
                await _next(context);
                return;
            }
            if (_options.PublicPaths.Any(p => path != null && path.StartsWith(p.ToLowerInvariant())))
            {
                await _next(context);
                return;
            }

            var accessToken = context.Request.Cookies[_options.AccessTokenCookieName];
            var refreshToken = context.Request.Cookies[_options.RefreshTokenCookieName];

            // first check if token is valid without expiration time(audience, issuer, key)
            try
            {
                var opt = context.RequestServices.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer");
                var validationParams = opt.TokenValidationParameters;
                validationParams.ValidateLifetime = false;

                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(accessToken, validationParams, out _);
            }
            catch (Exception ex)
            {
                // if token is not valid - it could be fake, so return 401
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // if token is valid with all params, check if it is expired and needs refresh
            bool shouldRefresh = false;
            if (string.IsNullOrEmpty(accessToken) || IsTokenExpired(accessToken))
                shouldRefresh = true;

            if (shouldRefresh)
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                // Отримуємо ідентифікатор користувача (наприклад, з refresh token або окремого cookie)
                var userId = GetUniqueKey(refreshToken);
                var cacheKey = $"token_refresh_{userId}";

                // Спроба отримати задачу оновлення з кешу
                if (!_cache.TryGetValue<Task<TokenResponse?>>(cacheKey, out var refreshTask))
                {
                    // Створюємо нову задачу оновлення
                    refreshTask = RefreshTokensAsync(refreshToken);
                    // Зберігаємо в кеші з коротким часом життя (наприклад, 10 секунд)
                    _cache.Set(cacheKey, refreshTask, TimeSpan.FromSeconds(10));
                }

                // Очікуємо на результат (всі запити, що прийшли одночасно, будуть чекати тут)
                var tokenResponse = await refreshTask;

                // Після завершення видаляємо з кешу (щоб наступні запити з новим refresh token створювали нову задачу)
                _cache.Remove(cacheKey);

                if (tokenResponse == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                // Remove old created values from login
                var standrdOptionsForRemove = new CookieOptions
                {
                    Path = "/",
                    SameSite = SameSiteMode.None,
                    HttpOnly = true,
                    Secure = true
                };

                context.Response.Cookies.Delete("access_token", standrdOptionsForRemove);
                context.Response.Cookies.Delete("refresh_token", standrdOptionsForRemove);

                // Зберігаємо нові токени в cookies
                SetTokenCookie(context, _options.AccessTokenCookieName, tokenResponse.AccessToken, TimeSpan.FromSeconds(tokenResponse.ExpiresIn));
                SetTokenCookie(context, _options.RefreshTokenCookieName, tokenResponse.RefreshToken, TimeSpan.FromDays(_options.RefreshTokenExpirationDays));

                // Оновлюємо токен для поточного запиту
                accessToken = tokenResponse.AccessToken;
                context.Request.Headers["Authorization"] = $"Bearer {accessToken}";
            }
            else
            {
                // Якщо токен валідний – просто додаємо заголовок
                context.Request.Headers["Authorization"] = $"Bearer {accessToken}";
            }

            await _next(context);
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                if (!jwtHandler.CanReadToken(token))
                    return true;

                var jwtToken = jwtHandler.ReadJwtToken(token);
                var expiration = jwtToken.ValidTo;
                return expiration <= DateTime.UtcNow.AddSeconds(_options.ExpirationLeewaySeconds);
            }
            catch
            {
                return true; // Якщо не вдалося розпарсити – вважаємо простроченим
            }
        }

        private async Task<TokenResponse?> RefreshTokensAsync(string refreshToken)
        {
            var client = _httpClientFactory.CreateClient();
            var requestUrl = $"{_options.AuthServiceUrl.TrimEnd('/')}/refresh-token";

            var requestBody = new RefreshTokenRequest { RefreshToken = refreshToken };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            try
            {
                var response = await client.PostAsync(requestUrl, content);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);
                return tokenResponse;
            }
            catch
            {
                return null;
            }
        }


        private string GetUniqueKey(string refreshToken)
        {
            return refreshToken.GetHashCode().ToString();
        }

        private void SetTokenCookie(HttpContext context, string cookieName, string token, TimeSpan expiresInSeconds)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Тільки HTTPS
                SameSite = SameSiteMode.Lax,
                MaxAge = expiresInSeconds
            };
            context.Response.Cookies.Append(cookieName, token, cookieOptions);
        }
    }
}