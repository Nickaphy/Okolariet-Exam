using System.ComponentModel.DataAnnotations;

namespace Api.Requests.AuthRequests
{
    public sealed record RefreshTokensRequest([property: Required] string RefreshToken);
}
