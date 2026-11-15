using Application.Auth;
using Application.Commands.AuthCommands;
using FluentResults;

namespace Application.InBoundPort.AuthUseCases
{
    public interface IRefreshTokensUseCase
    {
        Task<Result<AuthTokens>> ExecuteAsync(RefreshTokenCommand command, CancellationToken ct = default);
    }
}
