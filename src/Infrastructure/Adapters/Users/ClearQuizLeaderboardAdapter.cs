using Application.OutBoundPort.IUserRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Users
{
    public sealed class ClearQuizLeaderboardAdapter(OkolarietDbContext db) : IClearQuizLeaderboardAsync
    {
        public async Task<bool> ClearAsync(Guid moddedTemplateId, CancellationToken ct)
        {
            var quizExists = await db.ModdedTemplates
                .AnyAsync(template => template.Id == moddedTemplateId, ct);

            if (!quizExists)
                return false;

            // An empty leaderboard is also a successful clear.
            await db.Users
                .Where(user => user.ModdedTemplateId == moddedTemplateId)
                .ExecuteDeleteAsync(ct);

            return true;
        }
    }
}
