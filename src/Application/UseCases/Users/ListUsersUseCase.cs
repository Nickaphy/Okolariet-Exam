using Application.ErrorMessages;
using Application.Helpers;
using Application.InBoundPort.IUserUsecases;
using Application.OutBoundPort.IUserRepository;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Users;

public class ListUsersUseCase(
    IListUsersAsync listUsers) : IListUsersUseCase
{
    public async Task<Result<PagedList<User>>> ListAsync(
        Guid? moddedTemplateId,
        string sort,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        // Protect Application when called independently of the controller.
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return Result.Fail<PagedList<User>>(
                new BusinessRuleError(
                    "Page must be positive and pageSize must be 1 to 100."));

        if (sort != "bestScore" && sort != "newest")
            return Result.Fail<PagedList<User>>(
                new BusinessRuleError(
                    "Sort must be bestScore or newest."));

        // Infrastructure performs filtering, ordering, and pagination.
        var entries = await listUsers.ListAsync(
            moddedTemplateId, sort, page, pageSize, ct);

        return Result.Ok(entries);
    }
}