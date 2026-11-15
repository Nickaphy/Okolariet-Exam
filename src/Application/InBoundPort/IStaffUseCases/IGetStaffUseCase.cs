using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IStaffUseCases
{
    public interface IGetStaffUseCase
    {
        Task<Result<StaffMember>> GetAsync(Guid id, CancellationToken ct);
    }
}
