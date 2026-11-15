namespace Application.Auth
{
    public sealed class JwtSettings
    {
        public string Secret { get; init; } = "";            // must be at least 32 characters long, otherwise the JWT library will throw an exception
        public string Issuer { get; init; } = "";
        public string Audience { get; init; } = "";
        public int AccessTokenMinutes { get; init; } = 15;   // Expires in 15 minutes
        public int RefreshTokenHours { get; init; } = 8;     // How long you can refresh the access token for, after that you need to login again. Expires in 8 hours
    }
}
