using Domain.Entities;

namespace Application.OutBoundPort.IStaffRepository
{
    public interface IGetByEmailAsync
    {
        Task<StaffMember?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);
    }
}
