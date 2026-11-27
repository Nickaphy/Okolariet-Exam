using Application.Commands.AssetCommands;        // DeleteAssetCommand
using Application.ErrorMessages;                 // NotFoundError, ConflictError
using Application.OutBoundPort.IAssetRepository; // IGetAssetByIdAsync, IAssetInUseAsync, IDeleteAssetAsync
using Application.OutBoundPort.IFileStorage;     // IDeleteFileAsync
using Application.UseCases.Assets;               // DeleteAssetUseCase
using Domain.Entities;                           // Asset
using Moq;

namespace MoqTests.UseCases.Assets
{
    public class DeleteAssetMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetAssetByIdAsync> getAsset = new();
        private readonly Mock<IAssetInUseAsync> assetInUse = new();
        private readonly Mock<IDeleteAssetAsync> deleteAsset = new();
        private readonly Mock<IDeleteFileAsync> deleteFile = new();

        private DeleteAssetUseCase CreateUseCase()
            => new(getAsset.Object, assetInUse.Object, deleteAsset.Object, deleteFile.Object);

        [Fact]
        public async Task Delete_removes_row_first_then_file()
        {
            var asset = AssetTestData.Image("stored-abc.png");
            var calls = new List<string>();

            getAsset.Setup(g => g.GetAssetByIdAsync(asset.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(asset);
            assetInUse.Setup(u => u.AssetInUseAsync(asset.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);
            deleteAsset.Setup(d => d.DeleteAssetAsync(asset, It.IsAny<CancellationToken>()))
                       .Callback(() => calls.Add("row"))
                       .Returns(Task.CompletedTask);
            deleteFile.Setup(f => f.DeleteFileAsync("stored-abc.png", It.IsAny<CancellationToken>()))
                      .Callback(() => calls.Add("file"))
                      .Returns(Task.CompletedTask);

            var result = await CreateUseCase().DeleteAsync(new DeleteAssetCommand(asset.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { "row", "file" }, calls);
        }

        [Fact]
        public async Task Delete_unknown_asset_is_NotFound_and_deletes_nothing()
        {
            getAsset.Setup(g => g.GetAssetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Asset?)null);

            var result = await CreateUseCase().DeleteAsync(new DeleteAssetCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
            deleteAsset.Verify(d => d.DeleteAssetAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()), Times.Never);
            deleteFile.Verify(f => f.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Delete_asset_in_use_is_Conflict_and_keeps_everything()
        {
            var asset = AssetTestData.Image();
            getAsset.Setup(g => g.GetAssetByIdAsync(asset.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(asset);
            assetInUse.Setup(u => u.AssetInUseAsync(asset.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

            var result = await CreateUseCase().DeleteAsync(new DeleteAssetCommand(asset.Id), Ct);

            Assert.True(result.HasError<ConflictError>());
            deleteAsset.Verify(d => d.DeleteAssetAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()), Times.Never);
            deleteFile.Verify(f => f.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}