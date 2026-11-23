using Application.OutBoundPort.INicknameWordRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Nickname
{
    public sealed class RemoveNicknameWordAdapter(OkolarietDbContext db) : IRemoveAsync
    {
        public async Task RemoveAsync(NicknameWord word, CancellationToken ct)
        {
            db.NicknameWords.Remove(word);
            await db.SaveChangesAsync(ct);
        }
    }
}
