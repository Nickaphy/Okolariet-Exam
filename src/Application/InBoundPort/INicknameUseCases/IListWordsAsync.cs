using Application.Helpers;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.INicknameUseCases
{
    public interface IListWordsAsync
    {
        Task<Result<PagedList<NicknameWord>>> ListWordsAsync(int page, int pageSize, CancellationToken ct);
    }
}
