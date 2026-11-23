using Application.OutBoundPort.INicknameWordRepository;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Nickname
{
    public sealed class CountActiveNicknameWordsAdapter(OkolarietDbContext db) : ICountActiveAsync
    {
        public Task<int> CountActiveAsync(CancellationToken ct) =>
            db.NicknameWords.CountAsync(w => w.IsActive, ct);
    }
}
