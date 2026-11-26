using Application.OutBoundPort.IUserRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Users;

// Implements the delete port using the database context supplied by dependency injection.
public sealed class DeleteUserAdapter(OkolarietDbContext db) : IDeleteUserAsync
{
    // Takes the ID of the saved score and a token for cancelling the database call.
    public async Task<bool> DeleteAsync(Guid userId, CancellationToken ct)
    {
        // Delete the matching leaderboard entry directly in SQL Server.
        // EF Core returns the number of rows affected by the delete.
        var deleted = await db.Users
            // Limit the delete to the entry whose ID matches the request.
            .Where(user => user.Id == userId)
            // Send the DELETE to SQL Server immediately. This call saves the deletion itself.
            .ExecuteDeleteAsync(ct);

        return deleted > 0; // Zero rows means the entry did not exist.
    }
}
