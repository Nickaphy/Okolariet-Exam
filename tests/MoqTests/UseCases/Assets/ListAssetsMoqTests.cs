using Application.Helpers;                      // PagedList
using Application.OutBoundPort.IAssetRepository; // IListAssetsAsync
using Application.UseCases.Assets;              // ListAssetsUseCase
using Domain.Entities;                          // Asset
using Domain.Enums;                             // AssetKind
using Moq;

namespace MoqTests.UseCases.Assets
{
    public class ListAssetsMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IListAssetsAsync> listAssets = new();

        private ListAssetsUseCase CreateUseCase() => new(listAssets.Object);

        [Fact]
        public async Task List_returns_the_page_from_the_repository()
        {
            var page = new PagedList<Asset>(new[] { AssetTestData.Image() }, 1, 20, 1);
            listAssets.Setup(l => l.ListAssetsAsync(null, 1, 20, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(page);

            var result = await CreateUseCase().ListAsync(null, 1, 20, Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(page, result.Value);
        }

        [Fact]
        public async Task List_passes_kind_and_paging_on_unchanged()
        {
            listAssets.Setup(l => l.ListAssetsAsync(It.IsAny<AssetKind?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new PagedList<Asset>(Array.Empty<Asset>(), 2, 10, 0));

            await CreateUseCase().ListAsync(AssetKind.Video, 2, 10, Ct);

            listAssets.Verify(l => l.ListAssetsAsync(AssetKind.Video, 2, 10, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}