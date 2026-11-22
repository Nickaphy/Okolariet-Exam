using Application.ErrorMessages;
using Application.InBoundPort.IUserUsecases;
using Application.OutBoundPort.IUserRepository;
using FluentResults;

namespace Application.UseCases.Users;

public class DeleteUserUseCase(
    IDeleteUserAsync deleteUser) : IDeleteUserUseCase
{
    public async Task<Result> DeleteAsync(
        Guid userId,
        CancellationToken ct)
    {
        // The adapter reports whether an entry was actually deleted.
        var deleted = await deleteUser.DeleteAsync(userId, ct);

        if (!deleted)
            return Result.Fail(
                new NotFoundError("Saved score not found."));
                
        return Result.Ok();
    }
}