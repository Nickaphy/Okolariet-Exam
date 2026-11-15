using Domain.Entities;

namespace Application.OutBoundPort.IStaffRepository
{
    public interface IGetByIdAsync
    {
        Task<StaffMember?> GetByIdAsync(Guid id, CancellationToken ct);
    }
}
