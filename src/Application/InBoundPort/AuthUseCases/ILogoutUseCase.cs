using Application.Commands.AuthCommands;
using FluentResults;

namespace Application.InBoundPort.AuthUseCases
{
    public interface ILogoutUseCase
    {
        Task<Result> ExecuteAsync(LogoutCommand command, CancellationToken ct = default);
    }
}
