using Domain.Entities;

namespace Application.OutBoundPort.IStaffRepository
{
    public interface IAddAsync
    {
        Task AddAsync(StaffMember staff, CancellationToken ct);
    }
}
