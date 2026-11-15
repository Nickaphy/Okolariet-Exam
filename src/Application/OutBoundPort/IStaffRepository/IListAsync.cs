using Application.Helpers;
using Domain.Entities;

namespace Application.OutBoundPort.IStaffRepository
{
    public interface IListAsync
    {
        Task<PagedList<StaffMember>> ListAsync(int page, int pageSize, CancellationToken ct);
    }
}
