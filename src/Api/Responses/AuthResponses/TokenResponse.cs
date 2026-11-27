namespace Api.Responses.AuthResponses
{
    // The answer to POST /auth/tokens (openapi: Token). Both expiry times are sent as dates:
    //   ExpiresAt        = when the access token (JWT) stops working, 15 minutes;
    //   RefreshExpiresAt = when the whole login ends and the user must log in again, 8 hours.
    // The editor uses them to refresh just before the access token runs out, and to know when the login is gone.
    public sealed record TokenResponse(
        string AccessToken,
        string TokenType,
        DateTimeOffset ExpiresAt,
        string RefreshToken,
        DateTimeOffset RefreshExpiresAt);
}
