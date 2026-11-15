using System.ComponentModel.DataAnnotations;

namespace Api.Requests.AuthRequests
{
    public sealed record LoginRequest(
        [property: Required, EmailAddress] string Email,
        [property: Required, MaxLength(128)] string Password);
}
