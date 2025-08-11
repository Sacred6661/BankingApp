using AuthServer.Models;
using Duende.IdentityModel.Client;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;

namespace AuthServer.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _configuration = configuration;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Models.RegisterRequest model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);
            
            // User role by default
            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Models.LoginRequest model)
        {
            var clientId = _configuration["IdentitySecrets:ClientId"];
            var clientSecret = _configuration["IdentitySecrets:ClientAppSecret"];
            var scope = _configuration["IdentitySecrets:Scope"];

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = $"{baseUrl}/connect/token";

            var pairs = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "password"),
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("username", model.Email),
                new("password", model.Password),
                new("scope", "openid profile roles api_gateway account_service transaction_service history_service offline_access")
            };

            var content = new FormUrlEncodedContent(pairs);
            var response = await client.PostAsync(tokenEndpoint, content);

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, responseString);
            }

            // Return JSON response message from Duendo
            return Content(responseString, "application/json");
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] Models.RefreshTokenRequest model)
        {
            var clientId = _configuration["IdentitySecrets:ClientId"];
            var clientSecret = _configuration["IdentitySecrets:ClientAppSecret"];

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var tokenEndpoint = $"{baseUrl}/connect/token";

            var pairs = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "refresh_token"),
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("refresh_token", model.RefreshToken)
            };

            var content = new FormUrlEncodedContent(pairs);

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(tokenEndpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, responseString);

            return Content(responseString, "application/json");
        }


    }
}
