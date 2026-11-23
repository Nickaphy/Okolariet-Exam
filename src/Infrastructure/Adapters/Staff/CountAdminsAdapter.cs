using Application.OutBoundPort.IStaffRepository;
using Domain.Enums;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Staff
{
    public sealed class CountAdminsAdapter(OkolarietDbContext db) : ICountAdminsAsync
    {
        public Task<int> CountAdminsAsync(CancellationToken ct) =>
            db.Staff.CountAsync(s => s.Role == StaffRole.Admin, ct);
    }
}
