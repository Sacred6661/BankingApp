using AccountService.Helpers;
using Common.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

LoggingSetup.ConfigureLogging(builder);

var jwtConfig = builder.Configuration.GetSection("Jwt");

var jwksUrl = jwtConfig["JwksUrl"];
var keys = await JwtHelper.FetchSigningKeysFromJwks(jwksUrl);

var frontendUrl = builder.Configuration["FrontendUrl"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtConfig["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,

            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };

        options.TokenValidationParameters.RoleClaimType = "role";
    });

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUsersOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

var app = builder.Build();

LoggingSetup.UseCorrelationLogging(app);

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("Authorization"))
    {
        var accessToken = context.Request.Cookies["access_token"];
        if (!string.IsNullOrEmpty(accessToken))
        {
            context.Request.Headers.Add("Authorization", $"Bearer {accessToken}");
        }
    }

    await next();
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();


namespace ApiGateway
{
    public partial class Program { }
}
