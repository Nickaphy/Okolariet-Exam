using Application.Helpers;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IUserUsecases;

public interface IListUsersUseCase
{
    Task<Result<PagedList<User>>> ListAsync(
        Guid? moddedTemplateId,
        string sort,
        int page,
        int pageSize,
        CancellationToken ct);
}