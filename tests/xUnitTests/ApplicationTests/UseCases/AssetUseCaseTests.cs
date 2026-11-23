using Application.Commands.AssetCommands;   // UploadAssetCommand, DeleteAssetCommand
using Application.ErrorMessages;            // NotFoundError, ConflictError, BusinessRuleError
using Application.UseCases.Assets;          // the asset use cases
using Domain.Enums;                         // AssetKind
using xUnitTests.ApplicationTests.Fakes;    // fakes

namespace xUnitTests.ApplicationTests.UseCases
{
    public class AssetUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = new(2026, 11, 21, 12, 0, 0, TimeSpan.Zero);

        private readonly FakeAssetRepository repo = new();
        private readonly FakeFileStorage files = new();

        private static UploadAssetCommand Upload(string contentType = "image/png", long size = 3, string name = "cat.png")
            => new(new MemoryStream([1, 2, 3]), name, contentType, size);

        // ---------- Upload ----------

        [Fact]
        public async Task Upload_saves_file_and_asset()
        {
            var useCase = new UploadAssetUseCase(files, repo, new FixedTimeProvider(Now));

            var result = await useCase.UploadAsync(Upload(), Ct);

            Assert.True(result.IsSuccess);
            var asset = Assert.Single(repo.Assets);
            Assert.Same(asset, result.Value);
            Assert.Equal(AssetKind.Image, asset.Kind);
            Assert.Equal(Now, asset.UploadedAt);
            Assert.True(files.Files.ContainsKey(asset.StoredName));
        }

        [Fact]
        public async Task Upload_with_unknown_content_type_fails_and_saves_nothing()
        {
            var useCase = new UploadAssetUseCase(files, repo, new FixedTimeProvider(Now));

            var result = await useCase.UploadAsync(Upload(contentType: "application/pdf"), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(repo.Assets);
            Assert.Empty(files.Files);
        }

        [Fact]
        public async Task Upload_too_large_fails_and_saves_nothing()
        {
            var useCase = new UploadAssetUseCase(files, repo, new FixedTimeProvider(Now));

            var result = await useCase.UploadAsync(Upload(size: 10_000_001), Ct);   // image limit is 10 MB

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(repo.Assets);
            Assert.Empty(files.Files);
        }

        [Fact]
        public async Task Upload_without_name_fails_with_BusinessRuleError()
        {
            var useCase = new UploadAssetUseCase(files, repo, new FixedTimeProvider(Now));

            var result = await useCase.UploadAsync(Upload(name: ""), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(repo.Assets);
        }

        // ---------- Get ----------

        [Fact]
        public async Task Get_returns_asset_and_its_content()
        {
            var asset = TestData.Asset();
            repo.Assets.Add(asset);
            files.Files[asset.StoredName] = [9, 8, 7];
            var useCase = new GetAssetUseCase(repo, files);

            var result = await useCase.GetAsync(asset.Id, Ct);

            Assert.True(result.IsSuccess);
            Assert.Same(asset, result.Value.asset);
            using var reader = new MemoryStream();
            await result.Value.Content.CopyToAsync(reader);
            Assert.Equal(new byte[] { 9, 8, 7 }, reader.ToArray());
        }

        [Fact]
        public async Task Get_unknown_asset_is_NotFound()
        {
            var useCase = new GetAssetUseCase(repo, files);

            var result = await useCase.GetAsync(Guid.NewGuid(), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Get_when_file_is_missing_on_disk_is_NotFound()
        {
            var asset = TestData.Asset();
            repo.Assets.Add(asset);   // row exists, but no file
            var useCase = new GetAssetUseCase(repo, files);

            var result = await useCase.GetAsync(asset.Id, Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        // ---------- List ----------

        [Fact]
        public async Task List_filters_by_kind()
        {
            repo.Assets.Add(TestData.Asset(AssetKind.Image));
            repo.Assets.Add(TestData.Asset(AssetKind.Video));
            repo.Assets.Add(TestData.Asset(AssetKind.Image));
            var useCase = new ListAssetsUseCase(repo);

            var result = await useCase.ListAsync(AssetKind.Image, 1, 50, Ct);

            Assert.Equal(2, result.Value.TotalCount);
            Assert.All(result.Value.Items, a => Assert.Equal(AssetKind.Image, a.Kind));
        }

        // ---------- Delete ----------

        [Fact]
        public async Task Delete_removes_row_and_file()
        {
            var asset = TestData.Asset();
            repo.Assets.Add(asset);
            files.Files[asset.StoredName] = [1];
            var useCase = new DeleteAssetUseCase(repo, repo, repo, files);

            var result = await useCase.DeleteAsync(new DeleteAssetCommand(asset.Id), Ct);

            Assert.True(result.IsSuccess);
            Assert.Empty(repo.Assets);
            Assert.Empty(files.Files);
        }

        [Fact]
        public async Task Delete_unknown_asset_is_NotFound()
        {
            var useCase = new DeleteAssetUseCase(repo, repo, repo, files);

            var result = await useCase.DeleteAsync(new DeleteAssetCommand(Guid.NewGuid()), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Delete_asset_in_use_is_Conflict_and_keeps_everything()
        {
            var asset = TestData.Asset();
            repo.Assets.Add(asset);
            repo.InUse.Add(asset.Id);
            files.Files[asset.StoredName] = [1];
            var useCase = new DeleteAssetUseCase(repo, repo, repo, files);

            var result = await useCase.DeleteAsync(new DeleteAssetCommand(asset.Id), Ct);

            Assert.True(result.HasError<ConflictError>());
            Assert.Single(repo.Assets);
            Assert.Single(files.Files);
        }
    }
}
