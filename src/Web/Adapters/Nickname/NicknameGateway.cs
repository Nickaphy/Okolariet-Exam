using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Helpers;
using Web.Ports.Nickname;

namespace Web.Adapters.Nickname
{
    public class NicknameGateway(INicknamesApi api) : INicknameGateway
    {
        public async Task<IReadOnlyList<NicknameWord>> GetSuggestionsAsync(CancellationToken ct = default)
        {
            var words = (await api.GetNicknameSuggestions(ct)).Unwrap();
            return words.ToList();
        }

        public async Task<PagedResult<NicknameWord>> ListWordsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var p = (await api.ListNicknameWords(page, pageSize, ct)).Unwrap();
            return new PagedResult<NicknameWord>(p.Items.ToList(), p.Page, p.PageSize, p.TotalCount);
        }

        public async Task<NicknameWord> CreateNicknameWord(NicknameWordInput body, CancellationToken cancellationToken = default)
            => (await api.CreateNicknameWord(body, cancellationToken)).Unwrap();

        public async Task DeleteNicknameWord(Guid nicknameWordId, CancellationToken cancellationToken = default)
            => (await api.DeleteNicknameWord(nicknameWordId, cancellationToken)).EnsureSuccess();
    }
}