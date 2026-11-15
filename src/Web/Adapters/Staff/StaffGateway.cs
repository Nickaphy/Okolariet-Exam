using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Helpers;
using Web.Ports.Staff;
using localStaff = Web.Api.Contracts.Staff; //System is struggling to find the generic Staff, so i enforced a using with a local variable. It's functionally still just a normal Staff

namespace Web.Adapters.Staff
{
    // This is the Staff Gateway that implements the staff contracts from IStaffGateway. It is responsible for calling the Staff API
    public class StaffGateway (IStaffApi api) : IStaffGateway
    {
        public async Task<PagedResult<localStaff>> ListStaff(int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var pageResult = (await api.ListStaff(page, pageSize, ct)).Unwrap();
            return new PagedResult<localStaff>(pageResult.Items.ToList(), pageResult.Page, pageResult.PageSize, pageResult.TotalCount);
        }
        public async Task<localStaff> GetCurrentStaff(CancellationToken cancellationToken = default)
        {
            var staff = (await api.GetCurrentStaff(cancellationToken)).Unwrap();
            return staff;
        }
        public async Task<localStaff> GetStaff(Guid staffId, CancellationToken cancellationToken = default)
        {
            var staff = (await api.GetStaff(staffId, cancellationToken)).Unwrap();
            return staff;
        }
        public async Task<localStaff> UpdateStaff(Guid staffId, StaffUpdate body, CancellationToken cancellationToken = default)
        {
            var staff = (await api.UpdateStaff(staffId, body, cancellationToken)).Unwrap();
            return staff;
        }
        public async Task<localStaff> CreateStaff(StaffCreate body, CancellationToken cancellationToken = default)
            => (await api.CreateStaff(body, cancellationToken)).Unwrap();
        public async Task DeleteStaff(Guid staffId, CancellationToken cancellationToken = default)
            => (await api.DeleteStaff(staffId, cancellationToken)).EnsureSuccess();
        public async Task SetStaffPassword(Guid staffId, PasswordChange body, CancellationToken cancellationToken = default)
            => (await api.SetStaffPassword(staffId, body, cancellationToken)).EnsureSuccess();
    }
}
