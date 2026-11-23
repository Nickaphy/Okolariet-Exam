using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Staff
{
    public sealed class UpdateStaffAdapter(OkolarietDbContext db) : IUpdateAsync
    {
        public async Task UpdateAsync(StaffMember staff, CancellationToken ct)
        {
            db.Staff.Update(staff);
            await db.SaveChangesAsync(ct);
        }
    }
}
