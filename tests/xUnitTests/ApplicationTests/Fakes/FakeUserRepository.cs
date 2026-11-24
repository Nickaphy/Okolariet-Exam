using Application.Helpers;
using Application.OutBoundPort.INicknameWordRepository;
using Application.OutBoundPort.IUserRepository;
using Domain.Entities;

namespace xUnitTests.ApplicationTests.Fakes
{
    public class FakeUserRepository :
        ICreateUserAsync, IGetUserByIdAsync, IListUsersAsync, IDeleteUserAsync,
        IClearQuizLeaderboardAsync, IGetAsync
    {
        public List<NicknameWord> Words { get; } = new();
        public User? User { get; set; }
        public Guid QuizId { get; set; }
        public (Guid? QuizId, string Sort, int Page, int PageSize)? ListRequest { get; private set; }

        public Task<NicknameWord?> GetAsync(Guid id, CancellationToken ct)
            => Task.FromResult(Words.FirstOrDefault(word => word.Id == id));

        public Task<bool> CreateUserAsync(User user, CancellationToken ct)
        {
            User = user;
            return Task.FromResult(true);
        }

        public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct)
            => Task.FromResult(User?.Id == userId ? User : null);

        public Task<PagedList<User>> ListAsync(Guid? moddedTemplateId, string sort, int page, int pageSize, CancellationToken ct)
        {
            ListRequest = (moddedTemplateId, sort, page, pageSize);
            return Task.FromResult(new PagedList<User>([], page, pageSize, 0));
        }

        public Task<bool> DeleteAsync(Guid userId, CancellationToken ct)
        {
            if (User?.Id != userId)
                return Task.FromResult(false);

            User = null;
            return Task.FromResult(true);
        }

        public Task<bool> ClearAsync(Guid moddedTemplateId, CancellationToken ct)
        {
            if (QuizId != moddedTemplateId)
                return Task.FromResult(false);

            User = null;
            return Task.FromResult(true);
        }
    }
}
