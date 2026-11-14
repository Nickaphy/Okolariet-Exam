using Refit;
using Web.Api.Contracts;
using Web.Ports.Helpers;

namespace Web.Ports.Nickname
{
    // This is the Nickname Gateway "contract" that comes from the generated .refitter file. It needs to hold the exact same methods as the generated .refitter file, but it is not allowed to use any Refit or HTTP types.
    public interface INicknameGateway
    {
        Task<IReadOnlyList<NicknameWord>> GetSuggestionsAsync(CancellationToken ct = default);
        Task<PagedResult<NicknameWord>> ListWordsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
        Task<NicknameWord> CreateNicknameWord(NicknameWordInput body, CancellationToken cancellationToken = default);
        Task DeleteNicknameWord(Guid nicknameWordId, CancellationToken cancellationToken = default);
    }
}
