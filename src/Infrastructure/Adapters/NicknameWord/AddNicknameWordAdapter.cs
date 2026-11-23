using Application.OutBoundPort.INicknameWordRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Nickname
{
    public sealed class AddNicknameWordAdapter(OkolarietDbContext db) : IAddAsync
    {
        public async Task AddAsync(NicknameWord word, CancellationToken ct)
        {
            db.NicknameWords.Add(word);
            await db.SaveChangesAsync(ct);
        }
    }
}
