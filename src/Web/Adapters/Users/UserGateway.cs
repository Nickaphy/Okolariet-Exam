using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Helpers;
using Web.Ports.Users;

namespace Web.Adapters.Users
{
    public class UserGateway(IUsersApi api) : IUserGateway
    {
        public async Task<PagedResult<User>> ListUsers(Guid? quizId, Sort sort, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var p = (await api.ListUsers(quizId, sort, page, pageSize, ct)).Unwrap();
            return new PagedResult<User>(p.Items.ToList(), p.Page, p.PageSize, p.TotalCount);
        }

        public async Task<User> CreateUser(UserCreate body, CancellationToken ct = default)
            => (await api.CreateUser(body, ct)).Unwrap();

        public async Task DeleteUser(Guid userId, CancellationToken ct = default)
            => (await api.DeleteUser(userId, ct)).EnsureSuccess();

        public async Task DeleteQuizUsers(Guid quizId, CancellationToken ct = default)
            => (await api.DeleteModdedTemplateUsers(quizId, ct)).EnsureSuccess();
    }
}
