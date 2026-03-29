namespace ApiGateway.Models
{    public class TokenRefreshOptions
    {
        public string[] PublicPaths { get; set; } = Array.Empty<string>();
        public string? AuthServiceUrl { get; set; }
        public string AccessTokenCookieName { get; set; } = "access_token";
        public string RefreshTokenCookieName { get; set; } = "refresh_token";
        public int ExpirationLeewaySeconds { get; set; } = 5; // За скільки секунд до закінчення починати оновлення
        public int RefreshTokenExpirationDays { get; set; } = 30; // Час життя refresh token (в секундах) – має відповідати налаштуванням Duende
    }
}
