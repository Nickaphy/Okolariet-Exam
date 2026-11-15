using Domain.Entities;

namespace Application.OutBoundPort.IStaffRepository
{
    public interface IDeleteAsync
    {
        Task DeleteAsync(StaffMember staff, CancellationToken ct);
    }
}
