using Application.OutBoundPort.INicknameWordRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Nickname
{
    public sealed class GetNicknameWordAdapter(OkolarietDbContext db) : IGetAsync
    {
        // Tracked on purpose: DeleteWordUseCase passes the result straight to IRemoveAsync.
        public Task<NicknameWord?> GetAsync(Guid id, CancellationToken ct) =>
            db.NicknameWords.FirstOrDefaultAsync(w => w.Id == id, ct);
    }
}
