using Application.OutBoundPort.INicknameWordRepository;
using Domain.Entities;

namespace xUnitTests.ApplicationTests.Fakes
{
    // Stand-in for the nickname word table: a plain list instead of a database.
    public class FakeNicknameWordRepository : IAddAsync, IExistsAsync, IGetAsync, IRemoveAsync, ICountActiveAsync
    {
        public List<NicknameWord> Words { get; } = new();

        public Task AddAsync(NicknameWord word, CancellationToken ct) { Words.Add(word); return Task.CompletedTask; }

        public Task<bool> ExistsAsync(string text, CancellationToken ct)
            => Task.FromResult(Words.Any(w => w.Text.Equals(text, StringComparison.OrdinalIgnoreCase)));

        public Task<NicknameWord?> GetAsync(Guid id, CancellationToken ct)
            => Task.FromResult(Words.FirstOrDefault(w => w.Id == id));

        public Task RemoveAsync(NicknameWord word, CancellationToken ct) { Words.Remove(word); return Task.CompletedTask; }

        public Task<int> CountActiveAsync(CancellationToken ct) => Task.FromResult(Words.Count(w => w.IsActive));
    }
}
