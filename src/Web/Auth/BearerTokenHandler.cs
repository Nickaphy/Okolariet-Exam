using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace Web.Auth;

/// <summary>
/// Adds the access token to every API call, refreshes it when it is about to
/// expire, and retries once if the server still answers 401.
/// Used by every Refit client except <see cref="Web.Api.IAuthApi"/>.
/// </summary>
public sealed class BearerTokenHandler(AuthService auth, NavigationManager nav) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        // The kiosk window only calls public endpoints. It must never touch the admin's
        // tokens: if it refreshed at the same moment as the admin window, the rotated
        // refresh token would be presented twice and the server would end the login.
        if (IsKioskWindow)
        {
            return await base.SendAsync(request, ct);
        }

        var token = await auth.GetAccessTokenAsync(ct);
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var canRetry = token is not null && IsReplayable(request.Content);
        if (canRetry && request.Content is not null)
        {
            await request.Content.LoadIntoBufferAsync();
        }

        var response = await base.SendAsync(request, ct);
        if (response.StatusCode != HttpStatusCode.Unauthorized || !canRetry)
        {
            return response;
        }

        var refreshed = await auth.RefreshAsync(token!, ct);
        if (refreshed is null)
        {
            // AuthService has signed out; the router sends the user to the login page.
            return response;
        }

        response.Dispose();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed);
        return await base.SendAsync(request, ct);
    }

    private bool IsKioskWindow =>
        nav.ToBaseRelativePath(nav.Uri).StartsWith("kiosk/", StringComparison.OrdinalIgnoreCase);

    // File uploads can be hundreds of MB; never buffer them just to be able to retry.
    private static bool IsReplayable(HttpContent? content) =>
        content is null or (not StreamContent and not MultipartContent);
}

/// <summary>
/// Adds the stored access token without refreshing. Only used for IAuthApi,
/// which the refresh logic itself depends on (avoids a circular dependency).
/// </summary>
public sealed class AttachAccessTokenHandler(TokenStore store) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (await store.GetAsync() is { } token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }

        return await base.SendAsync(request, ct);
    }
}
