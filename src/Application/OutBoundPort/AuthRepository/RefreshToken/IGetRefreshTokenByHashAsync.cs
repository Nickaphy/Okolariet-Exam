using refreshToken = Domain.Entities.AuthEntities.RefreshToken; // Unable to find Refreshtoken, so had to be done explicitly

namespace Application.OutBoundPort.AuthRepository.RefreshToken
{
    public interface IGetRefreshTokenByHashAsync
    {
        Task<refreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default);
    }
}
