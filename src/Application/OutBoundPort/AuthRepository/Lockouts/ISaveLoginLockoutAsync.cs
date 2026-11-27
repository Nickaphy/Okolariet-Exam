using Domain.Entities.AuthEntities;

namespace Application.OutBoundPort.AuthRepository.Lockouts
{
    public interface ISaveLoginLockoutAsync      // insert or update
    {
        Task SaveLoginLockoutAsync(LoginLockout lockout, CancellationToken ct = default);
    }
}
