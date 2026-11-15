using Application.Commands.StaffCommands;
using FluentResults;

namespace Application.InBoundPort.IStaffUseCases
{
    public interface IDeleteStaffUseCase
    {
        Task<Result> DeleteAsync(DeleteStaffCommand command, CancellationToken ct);
    }
}
