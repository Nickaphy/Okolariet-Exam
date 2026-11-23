using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Staff
{
    public sealed class GetStaffByIdAdapter(OkolarietDbContext db) : IGetByIdAsync
    {
        // Tracked on purpose: Update, Delete and SetPassword change the result and pass it on.
        public Task<StaffMember?> GetByIdAsync(Guid id, CancellationToken ct) =>
            db.Staff.FirstOrDefaultAsync(s => s.Id == id, ct);
    }
}
