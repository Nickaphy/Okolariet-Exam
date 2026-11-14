using FluentResults;
using ReactiveUI.Primitives.Advanced;
using Web.Api;
using Web.Api.Contracts;

namespace Web.Auth
{
    // This service handles Authentication for the application. It is responsible for managing the access and refresh tokens, and ensuring that API requests are authenticated.
    // It uses a TokenStore to manage the tokens and their expiration time, and the generated IAuthApi to communicate with the authentication API.
    public class AuthHandler(TokenStore store, IAuthApi auth) : DelegatingHandler
    {
        private static readonly SemaphoreSlim Gate = new(1, 1);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage req, CancellationToken ct)
        {
            if (store.RefreshToken is not null && store.ExpiresAt < DateTimeOffset.UtcNow.AddSeconds(30))
            {
                var refreshResult = await RefreshAsync(ct);
                if (!refreshResult.IsSuccess)
                    throw new InvalidOperationException($"Failed to refresh token: {refreshResult.Errors.FirstOrDefault()?.Message}");
            }

            if (store.AccessToken is not null)
                req.Headers.Authorization = new("Bearer", store.AccessToken);

            var result = await base.SendAsync(req, ct);
            return result;
        }

        private async Task<Result> RefreshAsync(CancellationToken ct)
        {
            await Gate.WaitAsync(ct); // only one refresh at a time
            try
            {
                if (store.ExpiresAt >= DateTimeOffset.UtcNow.AddSeconds(30)) // someone else did it
                    return Result.Ok();

                var res = await auth.CreateToken(new TokenRequest
                {
                    GrantType = TokenRequestGrantType.Refresh_token,
                    RefreshToken = store.RefreshToken ?? throw new InvalidOperationException("Refresh token is null")
                }, ct);
                if (res.IsSuccessStatusCode)
                    store.Set(res.Content!);
                else
                    store.Clear(); // rotation or theft detection: force a new login

                return Result.Ok();
            }
            catch (InvalidOperationException ex)
            {
                store.Clear(); // force a new login
                return Result.Fail(ex.Message);
            }
            finally 
            { 
                Gate.Release(); 
            }
        }
    }
}
