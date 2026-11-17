using Application.Commands.AssetCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IAssetUseCases
{
    public interface IDeleteAssetUseCase
    {
        Task<Result> DeleteAsync(DeleteAssetCommand command, CancellationToken ct);
    }
}
