using Domain.Enums;

namespace Application.Commands.StaffCommands
{
    public record UpdateStaffCommand(Guid Id, string Email, StaffRole Role);
}
