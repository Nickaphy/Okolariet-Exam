using Application.Commands;
using FluentResults;

namespace Application.InBoundPort.INicknameUseCases
{
    public interface IDeleteWordAsync
    {
        Task<Result> DeleteWordAsync(DeleteNicknameWordCommand command, CancellationToken ct);
    }
}
