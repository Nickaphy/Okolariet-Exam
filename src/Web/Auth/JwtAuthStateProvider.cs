using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Auth;

/// <summary>
/// Tells Blazor who is logged in by reading the claims inside the stored JWT
/// (<c>sub</c>, <c>email</c>, <c>role</c>, <c>sid</c>). The token's signature is
/// checked by the API, not here; this only drives what the UI shows.
/// </summary>
public sealed class JwtAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly TokenStore _store;
    private readonly AuthService _auth;

    public JwtAuthStateProvider(TokenStore store, AuthService auth)
    {
        _store = store;
        _auth = auth;
        _auth.AuthenticationChanged += OnAuthenticationChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _store.GetAsync();

        // An expired access token is fine: the handler refreshes it on the next call.
        if (token is null || token.RefreshExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Anonymous;
        }

        var claims = ReadClaims(token.AccessToken);
        if (claims.Count == 0) return Anonymous;

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt", nameType: "email", roleType: "role");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private void OnAuthenticationChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static List<Claim> ReadClaims(string jwt)
    {
        var claims = new List<Claim>();
        var parts = jwt.Split('.');
        if (parts.Length < 2) return claims;

        try
        {
            using var doc = JsonDocument.Parse(Base64UrlDecode(parts[1]));
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                var type = property.Name == ClaimTypes.Role ? "role" : property.Name;

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in property.Value.EnumerateArray())
                        claims.Add(new Claim(type, ClaimValue(item)));
                }
                else
                {
                    claims.Add(new Claim(type, ClaimValue(property.Value)));
                }
            }
        }
        catch (Exception e) when (e is FormatException or JsonException)
        {
            claims.Clear();
        }

        return claims;
    }

    private static string ClaimValue(JsonElement value) =>
        value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText();

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
        return Convert.FromBase64String(s);
    }

    public void Dispose() => _auth.AuthenticationChanged -= OnAuthenticationChanged;
}
