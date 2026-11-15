using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Api.Requests.StaffRequests
{
    // Enforces basic validation rules -> can't be too short or too long.
    public record CreateStaffRequest(
        [property: Required, EmailAddress, MaxLength(254)] string Email,
        [property: Required, MinLength(12), MaxLength(128)] string Password,
        StaffRole Role);
}
