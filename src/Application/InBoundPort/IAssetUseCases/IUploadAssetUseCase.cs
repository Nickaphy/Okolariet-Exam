using Application.Commands.AssetCommands;
using Domain.Entities;
using FluentResults;

namespace Application.InBoundPort.IAssetUseCases
{
    public interface IUploadAssetUseCase
    {
        Task<Result<Asset>> UploadAsync(UploadAssetCommand command, CancellationToken ct);
    }
}