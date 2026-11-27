using Web.Api.Contracts;

namespace Web.Ports.Auth
{
    // Log in, renew a login and log out (POST /auth/tokens and DELETE /auth/tokens/current).
    // Only AuthService uses it. Errors come as GatewayException, like every other gateway
    // (401 = wrong e-mail/password or the login is gone, 429 = locked, with RetryAfter).
    public interface IAuthGateway
    {
        Task<Token> Login(string email, string password, CancellationToken ct = default);
        Task<Token> Refresh(string refreshToken, CancellationToken ct = default);
        Task Logout(CancellationToken ct = default);
    }
}
