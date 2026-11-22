using Application.Commands.Sessions;
using FluentResults;
using Domain.Entities;

namespace Application.InBoundPort;

public interface IPostSessionAsync
{
    Task<Result<Session>> PostSessionAsync(
        CreateSessionCommand command,
        CancellationToken ct);
}