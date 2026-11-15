using Web.Api.Contracts;
using Web.Ports.Helpers;
using localStaff = Web.Api.Contracts.Staff;

namespace Web.Ports.Staff
{
    public interface IStaffGateway
    {
        // Staff contracts based off the generated .refitter file. It needs to hold the exact same methods as the generated .refitter file, but it is not allowed to use any Refit or HTTP types.
        Task<PagedResult<localStaff>> ListStaff(int page = 1, int pageSize = 20, CancellationToken ct = default);
        Task<localStaff> CreateStaff(StaffCreate body, CancellationToken cancellationToken = default);
        Task<localStaff> GetCurrentStaff(CancellationToken cancellationToken = default);
        Task<localStaff> GetStaff(Guid staffId, CancellationToken cancellationToken = default);
        Task<localStaff> UpdateStaff(Guid staffId, StaffUpdate body, CancellationToken cancellationToken = default);
        Task DeleteStaff(Guid staffId, CancellationToken cancellationToken = default);
        Task SetStaffPassword(Guid staffId, PasswordChange body, CancellationToken cancellationToken = default);
    }
}
