namespace Application.Commands.StaffCommands
{
    public record DeleteStaffCommand(Guid Id, Guid RequestedById);
}
