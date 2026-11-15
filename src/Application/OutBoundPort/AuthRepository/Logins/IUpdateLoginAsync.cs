using Domain.Entities.AuthEntities;

namespace Application.OutBoundPort.AuthRepository.Logins
{
    public interface IUpdateLoginAsync
    {
        Task UpdateLoginAsync(StaffLogin login, CancellationToken ct = default);
    }
}
