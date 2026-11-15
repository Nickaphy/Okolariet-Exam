namespace Api.Responses.AuthResponses
{ // Only strictly important information about the token. 
    public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn, string RefreshToken);
}
