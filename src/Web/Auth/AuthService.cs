using System.Net;
using Web.Api.Contracts;
using Web.Exceptions;
using Web.Ports.Auth;

namespace Web.Auth;

public enum LoginOutcome { Success, InvalidCredentials, TooManyAttempts, Offline, Failed }

public sealed record LoginResult(LoginOutcome Outcome, TimeSpan? RetryAfter = null);

// Login, token refresh and logout for staff. Talks to the API through IAuthGateway.
//
// How the tokens work (the server's rules, see the contract):
// - the access token (a JWT) is sent with every call and lasts 15 minutes;
// - the refresh token renews it and lasts until the login ends (8 hours). It can be used ONCE: every refresh
//   gives a new one, and presenting an old one again makes the server end the whole login (theft protection).
// That last rule is why only one refresh may run at a time, and why the tokens are always read fresh from
// TokenStore (another window of the app may have refreshed a moment ago).
public sealed class AuthService(IAuthGateway gateway, TokenStore store)
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    // Refresh a little before the access token expires, so a call never arrives with a token that just ran out.
    private static readonly TimeSpan ExpirySkew = TimeSpan.FromSeconds(30);

    // Raised when the user logs in, logs out or the login is lost. JwtAuthStateProvider listens.
    public event Action? AuthenticationChanged;

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        try
        {
            await SaveAsync(await gateway.Login(email, password, ct));
            AuthenticationChanged?.Invoke();
            return new LoginResult(LoginOutcome.Success);
        }
        catch (GatewayException e)
        {
            return e.StatusCode switch
            {
                // Same answer whether the e-mail exists or not, like the API.
                HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest => new LoginResult(LoginOutcome.InvalidCredentials),
                // 5 wrong passwords: locked for 15 minutes. The server says how long in Retry-After.
                HttpStatusCode.TooManyRequests => new LoginResult(LoginOutcome.TooManyAttempts, e.RetryAfter),
                _ => new LoginResult(LoginOutcome.Failed)
            };
        }
        catch (HttpRequestException)
        {
            return new LoginResult(LoginOutcome.Offline);
        }
    }

    // An access token that is valid for at least a few more seconds, refreshing first if needed.
    // null = not logged in. Called by BearerTokenHandler before every API call.
    public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var current = await store.GetAsync();
        if (current is null) return null;
        if (current.ExpiresAt - ExpirySkew > DateTimeOffset.UtcNow) return current.AccessToken;

        return await RefreshAsync(current.AccessToken, ct);
    }

    // Called when the server rejected rejectedAccessToken (or it is about to expire).
    // Returns a new access token, or null if the login is gone.
    public async Task<string?> RefreshAsync(string rejectedAccessToken, CancellationToken ct = default)
    {
        await _refreshLock.WaitAsync(ct);
        try
        {
            var current = await store.GetAsync();
            if (current is null) return null;

            // Someone else (another call, or another window) refreshed while we waited: use their new token.
            if (current.AccessToken != rejectedAccessToken && current.ExpiresAt - ExpirySkew > DateTimeOffset.UtcNow)
                return current.AccessToken;

            // The 8-hour login is over: no point asking the server.
            if (current.RefreshExpiresAt <= DateTimeOffset.UtcNow)
            {
                await SignOutLocallyAsync();
                return null;
            }

            var token = await gateway.Refresh(current.RefreshToken, ct);
            await SaveAsync(token);
            return token.AccessToken;
        }
        catch (GatewayException e) when (e.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
        {
            // The server ended the login (logged out, password changed, token reused). Forget it here too.
            await SignOutLocallyAsync();
            return null;
        }
        catch (Exception e) when (e is GatewayException or HttpRequestException)
        {
            // 429, 5xx or offline: keep the login and let this one call fail. The next call tries again.
            return null;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    // Never throws: without a connection the login is simply forgotten here (its refresh token expires on its own).
    public async Task LogoutAsync()
    {
        try
        {
            // Make sure the access token is fresh, so the server accepts the logout.
            if (await GetAccessTokenAsync() is not null)
                await gateway.Logout();
        }
        catch (Exception e) when (e is GatewayException or HttpRequestException)
        {
            // Already logged out on the server, or offline. Forget the login locally anyway.
        }

        await SignOutLocallyAsync();
    }

    private async Task SignOutLocallyAsync()
    {
        await store.ClearAsync();
        AuthenticationChanged?.Invoke();
    }

    private ValueTask SaveAsync(Token token) => store.SetAsync(new StoredToken(
        token.AccessToken, token.ExpiresAt, token.RefreshToken, token.RefreshExpiresAt));
}
