using Application.Helpers;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IStaffUseCases
{
    public interface IListStaffUseCase
    {
        Task<Result<PagedList<StaffMember>>> ListAsync(int page, int pageSize, CancellationToken ct);
    }
}
