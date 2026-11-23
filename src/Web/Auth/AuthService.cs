using System.Net;
using Web.Api;
using Web.Api.Contracts;

namespace Web.Auth;

public enum LoginOutcome { Success, InvalidCredentials, TooManyAttempts, Offline, Failed }

public sealed record LoginResult(LoginOutcome Outcome, TimeSpan? RetryAfter = null);

/// <summary>
/// Login, token refresh and logout against <c>/auth/tokens</c>.
/// </summary>
public sealed class AuthService(IAuthApi authApi, TokenStore store)
{
    // Refresh tokens rotate, and presenting a used one revokes the login,
    // so only one refresh may be in flight at a time.
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private static readonly TimeSpan ExpirySkew = TimeSpan.FromSeconds(30);

    /// <summary>Raised when the user logs in, logs out or the login is lost.</summary>
    public event Action? AuthenticationChanged;

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        try
        {
            var response = await authApi.CreateToken(new TokenRequest
            {
                GrantType = TokenRequestGrantType.Password,
                Email = email,
                Password = password
            }, ct);

            if (response.IsSuccessStatusCode && response.Content is { } token)
            {
                await SaveAsync(token);
                AuthenticationChanged?.Invoke();
                return new LoginResult(LoginOutcome.Success);
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest =>
                    new LoginResult(LoginOutcome.InvalidCredentials),
                HttpStatusCode.TooManyRequests =>
                    new LoginResult(LoginOutcome.TooManyAttempts, response.Headers.RetryAfter?.Delta),
                _ => new LoginResult(LoginOutcome.Failed)
            };
        }
        catch (HttpRequestException)
        {
            return new LoginResult(LoginOutcome.Offline);
        }
    }

    /// <summary>
    /// Returns an access token that is valid for at least a few more seconds,
    /// refreshing first if needed. Null means "not logged in".
    /// </summary>
    public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var current = await store.GetAsync();
        if (current is null) return null;
        if (current.ExpiresAt - ExpirySkew > DateTimeOffset.UtcNow) return current.AccessToken;

        return await RefreshAsync(current.AccessToken, ct);
    }

    /// <summary>
    /// Called when the server rejected <paramref name="rejectedAccessToken"/>.
    /// Returns a new access token, or null if the login is gone.
    /// </summary>
    public async Task<string?> RefreshAsync(string rejectedAccessToken, CancellationToken ct = default)
    {
        await _refreshLock.WaitAsync(ct);
        try
        {
            var current = await store.GetAsync();
            if (current is null) return null;

            // Someone else (another request or another window) already refreshed.
            if (current.AccessToken != rejectedAccessToken
                && current.ExpiresAt - ExpirySkew > DateTimeOffset.UtcNow)
            {
                return current.AccessToken;
            }

            if (current.RefreshExpiresAt <= DateTimeOffset.UtcNow)
            {
                await SignOutLocallyAsync();
                return null;
            }

            var response = await authApi.CreateToken(new TokenRequest
            {
                GrantType = TokenRequestGrantType.Refresh_token,
                RefreshToken = current.RefreshToken
            }, ct);

            if (response.IsSuccessStatusCode && response.Content is { } token)
            {
                await SaveAsync(token);
                return token.AccessToken;
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
            {
                await SignOutLocallyAsync();
            }

            // Other failures (429, 5xx): keep the stored login and let the caller fail this once.
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Make sure the stored access token is fresh so the server accepts the logout.
            if (await GetAccessTokenAsync() is not null)
            {
                await authApi.DeleteCurrentToken();
            }
        }
        catch (HttpRequestException)
        {
            // Offline: forget the login locally anyway. The refresh token expires on its own.
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
