using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Staff
{
    public sealed class DeleteStaffAdapter(OkolarietDbContext db) : IDeleteAsync
    {
        public async Task DeleteAsync(StaffMember staff, CancellationToken ct)
        {
            db.Staff.Remove(staff);
            await db.SaveChangesAsync(ct);
        }
    }
}
