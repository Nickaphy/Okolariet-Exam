using Domain.Entities.AuthEntities;

namespace Application.OutBoundPort.AuthRepository.Logins
{
    public interface IAddLoginAsync
    {
        Task AddLoginAsync(StaffLogin login, CancellationToken ct = default);
    }
}
