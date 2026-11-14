using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.INicknameUseCases
{
    public interface IGetSuggestionsAsync
    {
        Task<Result<IReadOnlyList<NicknameWord>>> GetSuggestionsAsync(CancellationToken ct);
    }
}
