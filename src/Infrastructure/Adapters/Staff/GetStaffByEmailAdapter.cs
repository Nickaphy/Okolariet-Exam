using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Staff
{
    public sealed class GetStaffByEmailAdapter(OkolarietDbContext db) : IGetByEmailAsync
    {
        public Task<StaffMember?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
            db.Staff.AsNoTracking().FirstOrDefaultAsync(s => s.Email == normalizedEmail, ct);
    }
}
