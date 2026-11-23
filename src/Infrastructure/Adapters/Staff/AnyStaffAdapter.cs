using Application.OutBoundPort.IStaffRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Staff
{
    // Used by the seeder: is there any staff member at all?
    public sealed class AnyStaffAdapter(OkolarietDbContext db) : IAnyAsync
    {
        public Task<bool> AnyAsync(CancellationToken ct) => db.Staff.AnyAsync(ct);
    }
}
