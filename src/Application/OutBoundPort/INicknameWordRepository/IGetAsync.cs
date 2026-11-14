using Domain.Entities;

namespace Application.OutBoundPort.INicknameWordRepository
{
    public interface IGetAsync
    {
        Task<NicknameWord?> GetAsync(Guid id, CancellationToken ct);
    }
}
