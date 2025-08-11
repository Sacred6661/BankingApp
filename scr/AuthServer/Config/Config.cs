using Duende.IdentityServer.Models;

namespace AuthServer.Config
{
    public static class Config
    {
        private static IConfiguration _configuration;

        public static void Init(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api_gateway", "API Gateway", new[] { "role", "user_id" }),
                new ApiScope("account_service", "Account Service", new[] { "role", "user_id" }),
                new ApiScope("transaction_service", "Transaction Service", new[] { "role", "user_id" }),
                new ApiScope("history_service", "History Service", new[] { "role", "user_id" })
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("api_gateway", "API Gateway")
                {
                    Scopes = { "api_gateway" },
                    UserClaims = { "role", "user_id" }
                },
                new ApiResource("account_service", "Account Service")
                {
                    Scopes = { "account_service" },
                    UserClaims = { "role", "user_id" }
                },  
                new ApiResource("transaction_service", "Transaction Service")
                {
                    Scopes = { "transaction_service" },
                    UserClaims = { "role", "user_id" }
                }             ,   
                new ApiResource("history_service", "History Service")
                {
                    Scopes = { "history_service" },
                    UserClaims = { "role", "user_id" }
                }
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
            new Client
            {
                ClientId = GetSecret("IdentitySecrets:ClientId"),
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = {
                    new Secret(GetSecret("IdentitySecrets:ClientAppSecret").Sha256())
                },
                AllowedScopes = { "api_gateway", "account_service", "transaction_service", "history_service", "openid", "profile", "roles", "user_id" },

                AlwaysIncludeUserClaimsInIdToken = true,
                AlwaysSendClientClaims = true,

                AllowOfflineAccess = true,

                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 1296000, 
                AbsoluteRefreshTokenLifetime = 2592000
            },

                // Опціонально, клієнт для API Gateway (якщо потрібно авторизувати його окремо)
                /*
                new Client
                {
                    ClientId = "gateway_client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("gateway_secret".Sha256()) },
                    AllowedScopes = { "orders_api", "account_service" }
                }
                */
            };

        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", "User roles", new[] { "role" }),
                new IdentityResource("user_id", "User ID", new[] { "user_id" })
            };

        private static string GetSecret(string key)
        {
            if (_configuration == null)
                throw new InvalidOperationException("Configuration not initialized");

            return _configuration[key];
        }
    }
}
