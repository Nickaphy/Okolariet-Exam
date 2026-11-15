using Application.Commands.StaffCommands;
using FluentResults;

namespace Application.InBoundPort.IStaffUseCases
{
    public interface ISetStaffPasswordUseCase
    {
        Task<Result> SetPasswordAsync(SetStaffPasswordCommand command, CancellationToken ct);
    }
}
