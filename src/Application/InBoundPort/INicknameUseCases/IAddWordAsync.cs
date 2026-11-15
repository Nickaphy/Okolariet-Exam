using Application.Commands.NicknameCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.INicknameUseCases
{
    public interface IAddWordAsync
    {
        Task<Result<NicknameWord>> AddWordAsync(AddNicknameWordCommand command, CancellationToken ct);
    }
}
