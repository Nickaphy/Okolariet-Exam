using System.ComponentModel.DataAnnotations;

namespace Api.Requests.StaffRequests
{
    // This is just basic validation, it goes into a lot more detail later in the use case.
    public record SetStaffPasswordRequest(
    string? CurrentPassword,
    [Required, MinLength(12), MaxLength(128)] string NewPassword);
}
