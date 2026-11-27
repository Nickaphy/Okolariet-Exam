using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Auth;

namespace Web.Adapters.Auth
{
    // One endpoint, two "grant types" (see the contract): password to log in, refresh_token to renew.
    //
    // Note: the Refit client behind this (IAuthApi) is registered with AttachAccessTokenHandler, NOT BearerTokenHandler.
    // BearerTokenHandler refreshes through AuthService, which uses this gateway, so using it here would go in a circle.
    public class AuthGateway(IAuthApi api) : IAuthGateway
    {
        public async Task<Token> Login(string email, string password, CancellationToken ct = default)
            => (await api.CreateToken(new TokenRequest
            {
                GrantType = TokenRequestGrantType.Password,
                Email = email,
                Password = password
            }, ct)).Unwrap();

        public async Task<Token> Refresh(string refreshToken, CancellationToken ct = default)
            => (await api.CreateToken(new TokenRequest
            {
                GrantType = TokenRequestGrantType.Refresh_token,
                RefreshToken = refreshToken
            }, ct)).Unwrap();

        public async Task Logout(CancellationToken ct = default)
            => (await api.DeleteCurrentToken(ct)).EnsureSuccess();
    }
}
