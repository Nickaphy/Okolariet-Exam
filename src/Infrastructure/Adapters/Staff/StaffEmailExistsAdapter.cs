using Application.OutBoundPort.IStaffRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Staff
{
    public sealed class StaffEmailExistsAdapter(OkolarietDbContext db) : IEmailExistsAsync
    {
        // exceptId: when updating, the staff member's own e-mail doesn't count as taken.
        public Task<bool> EmailExistsAsync(string normalizedEmail, Guid? exceptId, CancellationToken ct) =>
            db.Staff.AnyAsync(s => s.Email == normalizedEmail && (exceptId == null || s.Id != exceptId), ct);
    }
}
