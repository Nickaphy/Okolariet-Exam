using Domain.Enums;

namespace Application.Commands.StaffCommands
{
    public record CreateStaffCommand(string Email, string Password, StaffRole Role);
}
