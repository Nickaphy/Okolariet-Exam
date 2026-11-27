using Application.ErrorMessages;                 // NotFoundError
using Application.OutBoundPort.IAssetRepository; // IGetAssetByIdAsync
using Application.OutBoundPort.IFileStorage;     // IOpenFileAsync
using Application.UseCases.Assets;               // GetAssetUseCase
using Domain.Entities;                           // Asset
using Moq;

namespace MoqTests.UseCases.Assets
{
    public class GetAssetMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetAssetByIdAsync> getAsset = new();
        private readonly Mock<IOpenFileAsync> openFile = new();

        private GetAssetUseCase CreateUseCase() => new(getAsset.Object, openFile.Object);

        [Fact]
        public async Task Get_returns_asset_and_its_content()
        {
            var asset = AssetTestData.Image();
            using var content = new MemoryStream(new byte[] { 1, 2, 3 });
            getAsset.Setup(g => g.GetAssetByIdAsync(asset.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(asset);
            openFile.Setup(o => o.OpenFileAsync(asset.StoredName, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(content);

            var result = await CreateUseCase().GetAsync(asset.Id, Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(asset, result.Value.asset);
            Assert.Same(content, result.Value.Content);
        }

        [Fact]
        public async Task Get_unknown_asset_is_NotFound_and_never_opens_a_file()
        {
            getAsset.Setup(g => g.GetAssetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Asset?)null);

            var result = await CreateUseCase().GetAsync(Guid.NewGuid(), Ct);

            Assert.True(result.HasError<NotFoundError>());
            openFile.Verify(o => o.OpenFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Get_when_file_is_missing_on_disk_is_NotFound()
        {
            var asset = AssetTestData.Image();
            getAsset.Setup(g => g.GetAssetByIdAsync(asset.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(asset);
            openFile.Setup(o => o.OpenFileAsync(asset.StoredName, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Stream?)null);

            var result = await CreateUseCase().GetAsync(asset.Id, Ct);

            Assert.True(result.HasError<NotFoundError>());
        }
    }
}