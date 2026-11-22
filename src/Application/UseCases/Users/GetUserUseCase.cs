using Application.ErrorMessages;
using Application.InBoundPort.IUserUsecases;
using Application.OutBoundPort.IUserRepository;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Users;

public class GetUserUseCase(
    IGetUserByIdAsync getUser) : IGetUserUseCase
{
    public async Task<Result<User>> GetAsync(
        Guid userId,
        CancellationToken ct)
    {
        // The retrieve the leaderboard entry from infrastructure
        var user = await getUser.GetByIdAsync(userId, ct);

        if (user is null)
            return Result.Fail<User>(
                new NotFoundError("Saved score not found."));

        // Return the retrieved leaderboard entry with HTTP 200 OK.
        return Result.Ok(user);
    }
}