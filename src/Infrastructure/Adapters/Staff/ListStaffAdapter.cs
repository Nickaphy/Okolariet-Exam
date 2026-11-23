using Application.Helpers;
using Application.OutBoundPort.IStaffRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Staff
{
    public sealed class ListStaffAdapter(OkolarietDbContext db) : IListAsync
    {
        public async Task<PagedList<StaffMember>> ListAsync(int page, int pageSize, CancellationToken ct)
        {
            var total = await db.Staff.CountAsync(ct);
            var items = await db.Staff
                .AsNoTracking()
                .OrderBy(s => s.Email)               // a fixed order, so pages don't overlap or skip anyone
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedList<StaffMember>(items, page, pageSize, total);
        }
    }
}
