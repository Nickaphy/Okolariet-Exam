using Application.Auth;
using Application.Commands.AuthCommands;
using FluentResults;

namespace Application.InBoundPort.AuthUseCases
{
    public interface ILoginUseCase
    {
        Task<Result<AuthTokens>> ExecuteAsync(LoginCommand command, CancellationToken ct = default);
    }
}
