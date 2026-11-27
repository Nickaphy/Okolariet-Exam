using Application.OutBoundPort.IUserRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Users
{
    // Saves a guest's highscore. The port promises: false (not an exception) when this session has
    // already saved a score for this quiz, so CreateUserUseCase can answer 409 Conflict.
    public sealed class CreateUserAdapter(OkolarietDbContext db) : ICreateUserAsync
    {
        public async Task<bool> CreateUserAsync(User user, CancellationToken ct)
        {
            // The normal case: check first.
            if (await db.Users.AnyAsync(u => u.SessionId == user.SessionId && u.ModdedTemplateId == user.ModdedTemplateId, ct))
                return false;

            db.Users.Add(user);
            try
            {
                await db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                // Two saves at the same moment: the unique index (SessionId, ModdedTemplateId) stopped the second one.
                // Detach it, so the failed insert isn't retried on the next save.
                db.Entry(user).State = EntityState.Detached;
                return false;
            }
        }
    }
}
