using Application.Helpers;
using Application.OutBoundPort.IUserRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Users;

public sealed class ListUsersAdapter(OkolarietDbContext db) : IListUsersAsync
{
    public async Task<PagedList<User>> ListAsync(
        Guid? moddedTemplateId, string sort, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking(); // Execute query with no tracking and retrieve users from the database
        if (moddedTemplateId is Guid quizId) //
            query = query.Where(user => user.ModdedTemplateId == quizId);

        var total = await query.CountAsync(ct);

        // Calculate the percentage from the two stored score fields so SQL Server can sort it.
        var sorted = sort == "bestScore"
            ? query.OrderByDescending(user => 100.0 * user.Score.Correct / user.Score.TotalQuestions)
            : query.OrderByDescending(user => user.CreatedAt);

        var items = await sorted
            .ThenBy(user => user.Id)
            .Skip((int)Math.Min((page - 1L) * pageSize, int.MaxValue))
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<User>(items, page, pageSize, total);
    }
}
