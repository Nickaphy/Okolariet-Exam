namespace Web.Auth
{
    using Web.Api.Contracts;

    // The token store handles the access and refresh tokens, and their expiration time. Used by the AuthHandler to manage authentication state.
    // Helps prevent a user from being logged out unexpectedly, and allows for automatic token refresh when needed.
    public class TokenStore
    {
        public string? AccessToken { get; private set; } // A user may be logged in, but not have an access token -> some API calls use allowanonymous
        public string? RefreshToken { get; private set; } // null if not logged in, or if the refresh token has been revoked
        public DateTimeOffset ExpiresAt { get; private set; }

        public bool IsLoggedIn => AccessToken is not null;
        public event Action? Changed;

        public void Set(Token token)
        {
            AccessToken = token.AccessToken;
            RefreshToken = token.RefreshToken;
            ExpiresAt = token.ExpiresAt;
            Changed?.Invoke();
        }

        public void Clear()
        {
            AccessToken = RefreshToken = null;
            ExpiresAt = default;
            Changed?.Invoke();
        }
    }
}
