using System.Text.Json;
using Microsoft.JSInterop;
using Web.Api;

namespace Web.Auth;

public sealed record StoredToken(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt);

/// <summary>
/// Keeps the login in localStorage so it survives a page reload and is shared
/// with other windows of the app. Registered as a singleton: HttpClientFactory
/// resolves message handlers in its own scope, so per-scope state would not be shared.
/// </summary>
public sealed class TokenStore(IJSRuntime js)
{
    private const string Key = "oekolariet.auth";

    // Always read from storage: another window may have rotated the refresh token,
    // and re-using an old one revokes the whole login.
    public async ValueTask<StoredToken?> GetAsync()
    {
        var json = await js.InvokeAsync<string?>("localStorage.getItem", Key);
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<StoredToken>(json, ApiJson.Options);
        }
        catch (JsonException)
        {
            await ClearAsync();
            return null;
        }
    }

    public ValueTask SetAsync(StoredToken token) =>
        js.InvokeVoidAsync("localStorage.setItem", Key, JsonSerializer.Serialize(token, ApiJson.Options));

    public ValueTask ClearAsync() =>
        js.InvokeVoidAsync("localStorage.removeItem", Key);
}