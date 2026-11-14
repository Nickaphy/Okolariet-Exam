using Domain.Entities;

namespace Application.OutBoundPort.INicknameWordRepository
{
    public interface IAddAsync
    {
        Task AddAsync(NicknameWord word, CancellationToken ct);
    }
}
