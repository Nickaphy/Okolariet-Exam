using System.Net.Http.Headers;

namespace Web.TestTokens
{
    public sealed class AuthHandlerTest(TokenStoreTest tokens) : DelegatingHandler // Only for testing purposes
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (tokens.AccessToken is { } token)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return base.SendAsync(request, ct);
        }
    }
}
