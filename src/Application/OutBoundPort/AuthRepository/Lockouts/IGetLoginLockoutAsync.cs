using Domain.ValueObjects;

namespace Application.OutBoundPort.AuthRepository.Lockouts
{
    public interface IGetLoginLockoutAsync
    {
        Task<LoginLockout?> GetLoginLockoutAsync(string normalizedEmail, CancellationToken ct = default);
    }
}
