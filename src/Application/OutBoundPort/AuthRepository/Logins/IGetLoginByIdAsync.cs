using Domain.Entities.AuthEntities;

namespace Application.OutBoundPort.AuthRepository.Logins
{
    public interface IGetLoginByIdAsync
    {
        Task<StaffLogin?> GetLoginByIdAsync(Guid id, CancellationToken ct = default);
    }
}
