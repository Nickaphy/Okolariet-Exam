using refreshToken = Domain.Entities.AuthEntities.RefreshToken; // Unable to find Refreshtoken, so had to be done explicitly

namespace Application.OutBoundPort.AuthRepository.RefreshToken
{
    public interface IUpdateRefreshTokenAsync
    {
        Task UpdateRefreshTokenAsync(refreshToken token, CancellationToken ct = default);
    }
}
