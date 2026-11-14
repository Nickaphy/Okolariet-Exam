using Domain.Entities;

namespace Application.OutBoundPort.INicknameWordRepository
{
    public interface IRemoveAsync
    {
        Task RemoveAsync(NicknameWord word, CancellationToken ct);
    }
}
