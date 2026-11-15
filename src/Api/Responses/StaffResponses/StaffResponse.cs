using Domain.Entities;
using Domain.Enums;

namespace Api.Responses.StaffResponses
{
    public record StaffResponse(Guid Id, string Email, StaffRole Role)   // no PasswordHash, ever
    {
        public static StaffResponse From(StaffMember s) => new(s.Id, s.Email, s.Role);
    }
}
