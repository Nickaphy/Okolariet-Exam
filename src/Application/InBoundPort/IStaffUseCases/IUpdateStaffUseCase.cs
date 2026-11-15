using Application.Commands.StaffCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IStaffUseCases
{
    public interface IUpdateStaffUseCase
    {
        Task<Result<StaffMember>> UpdateAsync(UpdateStaffCommand command, CancellationToken ct);
    }
}
