using System.ComponentModel.DataAnnotations;

namespace Api.Requests.AuthRequests
{
    public sealed record RefreshTokensRequest([Required] string RefreshToken);
}
