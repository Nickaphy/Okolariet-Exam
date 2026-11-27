namespace Web.TestTokens
{
    public sealed class TokenStoreTest // Only for testing purposes!
    {
        public string? AccessToken { get; private set; }
        public void Set(string accessToken) => AccessToken = accessToken;
        public void Clear() => AccessToken = null;
    }
}
