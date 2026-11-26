using Application.OutBoundPort.IUserRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Users;

// Implements the port that the Get User use case calls to read a saved score.
public sealed class GetUserByIdAdapter(OkolarietDbContext db) : IGetUserByIdAsync
{
    // This is a read-only lookup, so EF Core does not track the entry.
    // FirstOrDefaultAsync returns null if no entry matches the ID.
    // Return the database task so the use case can await the result.
    public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct) =>
        // Search the Users table by ID and pass the cancellation token to EF Core.
        db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == userId, ct);
}
