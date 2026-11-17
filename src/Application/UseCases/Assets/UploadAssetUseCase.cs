using Application.OutBoundPort.IAssetRepository;
using Application.InBoundPort.IAssetUseCases;
using Application.Commands.AssetCommands;
using Application.OutBoundPort.IFileStorage;
using Application.ErrorMessages;
using Domain.Entities;
using FluentResults;

namespace Application.UseCases.Assets
{
public class UploadAssetUseCase(
    ISaveFileAsync saveFile,
    IAddAssetAsync addAsset,
    TimeProvider time) : IUploadAssetUseCase
    {
        public async Task<Result<Asset>> UploadAsync(UploadAssetCommand cmd, CancellationToken ct)
        {
            try
            {
                var kind = Asset.KindFromContentType(cmd.ContentType);
                if (kind is null)
                    return Result.Fail(new BusinessRuleError("File type not allowed."));        // should become 415

                if (cmd.SizeBytes > Asset.MaxBytesFor(kind.Value))
                    return Result.Fail(new BusinessRuleError("File is too large."));            // should become 413

                var storedName = await saveFile.SaveFileAsync(cmd.Content, cmd.ContentType, ct);
                var asset = Asset.Create(kind.Value, cmd.OriginalName, storedName, cmd.ContentType, cmd.SizeBytes, time.GetUtcNow());
                await addAsset.AddAssetAsync(asset, ct);
                return Result.Ok(asset);
            }
            catch (ArgumentException ex)
            {
                return Result.Fail(new BusinessRuleError(ex.Message));
            }
        }
    }
}