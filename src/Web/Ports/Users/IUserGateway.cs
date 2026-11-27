using Web.Api.Contracts;
using Web.Ports.Helpers;

namespace Web.Ports.Users
{
    // Saved quiz scores ("users": a nickname and a score, no personal data).
    // The kiosk saves and lists them (anonymous); staff clear them (admin).
    public interface IUserGateway
    {
        Task<PagedResult<User>> ListUsers(Guid? quizId, Sort sort, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<User> CreateUser(UserCreate body, CancellationToken ct = default);
        Task DeleteUser(Guid userId, CancellationToken ct = default);
        Task DeleteQuizUsers(Guid quizId, CancellationToken ct = default);
    }
}
