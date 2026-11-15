using refreshToken = Domain.Entities.AuthEntities.RefreshToken; // Unable to find Refreshtoken, so had to be done explicitly

namespace Application.OutBoundPort.AuthRepository.RefreshToken
{
    public interface IAddRefreshTokenAsync
    {
        Task AddRefreshTokenAsync(refreshToken token, CancellationToken ct = default);
    }
}
