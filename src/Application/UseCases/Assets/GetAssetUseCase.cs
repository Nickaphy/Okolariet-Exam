using Application.OutBoundPort.IAssetRepository;
using Application.InBoundPort.IAssetUseCases;
using Application.OutBoundPort.IFileStorage;  
using Domain.Entities;
using FluentResults;
using Application.ErrorMessages;

namespace Application.UseCases.Assets
{
    public class GetAssetUseCase(IGetAssetByIdAsync getAsset, IOpenFileAsync openFile) : IGetAssetUseCase
    {
        public async Task<Result<(Asset asset, Stream Content)>> GetAsync(Guid id, CancellationToken ct)
        {
            var asset = await getAsset.GetAssetByIdAsync(id, ct);
            if (asset is null)
                return Result.Fail(new NotFoundError("Asset not found."));

            var stream = await openFile.OpenFileAsync(asset.StoredName, ct);
            if (stream is null)
                return Result.Fail(new NotFoundError("Asset file is missing."));

            return Result.Ok((asset, stream));
        }
    }
}