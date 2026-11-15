using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Api.Requests.StaffRequests
{
    public record UpdateStaffRequest(
    [property: Required, EmailAddress, MaxLength(254)] string Email,
    StaffRole Role);
}
