using Application.OutBoundPort.INicknameWordRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Nickname
{
    public sealed class NicknameWordExistsAdapter(OkolarietDbContext db) : IExistsAsync
    {
        // ToLower on both sides, so the check ignores case on every database, not only SQL Server.
        public Task<bool> ExistsAsync(string text, CancellationToken ct) =>
            db.NicknameWords.AnyAsync(w => w.Text.ToLower() == text.ToLower(), ct);
    }
}
