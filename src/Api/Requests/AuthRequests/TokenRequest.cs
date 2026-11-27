using System.ComponentModel.DataAnnotations;

namespace Api.Requests.AuthRequests
{
    // POST /auth/tokens (openapi: TokenRequest). One request, two "grant types", like OAuth 2:
    //   "password":      log in with Email and Password;
    //   "refresh_token": swap a RefreshToken for a new pair of tokens.
    // Which other fields are needed depends on GrantType, so AuthController checks them, not attributes.
    public sealed record TokenRequest(
        [Required] string GrantType,
        string? Email,
        [MaxLength(128)] string? Password,
        string? RefreshToken)
    {
        public const string PasswordGrant = "password";
        public const string RefreshTokenGrant = "refresh_token";
    }
}
