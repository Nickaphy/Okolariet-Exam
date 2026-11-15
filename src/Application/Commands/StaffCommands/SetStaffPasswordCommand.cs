namespace Application.Commands.StaffCommands
{
    public record SetStaffPasswordCommand(Guid Id, Guid RequestedById, string? CurrentPassword, string NewPassword);
}
