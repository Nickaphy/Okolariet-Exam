using System.ComponentModel.DataAnnotations;

namespace Api.Requests.AuthRequests
{
    public sealed record LoginRequest(
        [Required, EmailAddress] string Email,
        [Required, MaxLength(128)] string Password);
}
