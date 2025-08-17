using AccountService.Helpers;
using Common.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

LoggingSetup.ConfigureLogging(builder);

var jwtConfig = builder.Configuration.GetSection("Jwt");

var jwksUrl = jwtConfig["JwksUrl"];
var keys = await JwtHelper.FetchSigningKeysFromJwks(jwksUrl);

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
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var authorizationHeader = context.Request.Headers["Authorization"].ToString();
        context.Request.Headers["Authorization"] = authorizationHeader;
    }

    await next();
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();


namespace ApiGateway
{
    public partial class Program { }
}
