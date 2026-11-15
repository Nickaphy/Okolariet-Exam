using Application.Commands.StaffCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IStaffUseCases
{
    public interface ICreateStaffUseCase
    {
        Task<Result<StaffMember>> CreateAsync(CreateStaffCommand command, CancellationToken ct);
    }
}
