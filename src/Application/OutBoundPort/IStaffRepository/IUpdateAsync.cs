using Domain.Entities;

namespace Application.OutBoundPort.IStaffRepository
{
    public interface IUpdateAsync
    {
        Task UpdateAsync(StaffMember staff, CancellationToken ct);
    }
}
