using Application.OutBoundPort.IAssetRepository;
using Application.InBoundPort.IAssetUseCases;
using Application.Commands.AssetCommands;
using Application.OutBoundPort.IFileStorage;
using Application.ErrorMessages;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Assets
{
    public class DeleteAssetUseCase(
    IGetAssetByIdAsync getAsset,
    IAssetInUseAsync assetInUse,
    IDeleteAssetAsync deleteAsset,
    IDeleteFileAsync deleteFile) : IDeleteAssetUseCase
    {
        public async Task<Result> DeleteAsync(DeleteAssetCommand cmd, CancellationToken ct)
        {
            var asset = await getAsset.GetAssetByIdAsync(cmd.AssetId, ct);
            if (asset is null)
                return Result.Fail(new NotFoundError("Asset not found."));

            if (await assetInUse.AssetInUseAsync(asset.Id, ct))
                return Result.Fail(new ConflictError("The asset is still used by a question, answer or template."));   // 409

            await deleteAsset.DeleteAssetAsync(asset, ct);          // row first...
            await deleteFile.DeleteFileAsync(asset.StoredName, ct); // ...then the file
            return Result.Ok();
        }
    }
}