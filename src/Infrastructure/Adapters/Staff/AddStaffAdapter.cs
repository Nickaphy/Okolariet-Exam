using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Staff
{
    public sealed class AddStaffAdapter(OkolarietDbContext db) : IAddAsync
    {
        public async Task AddAsync(StaffMember staff, CancellationToken ct)
        {
            db.Staff.Add(staff);
            await db.SaveChangesAsync(ct);
        }
    }
}
