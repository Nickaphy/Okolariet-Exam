using Application.Commands.AssetCommands;        // UploadAssetCommand
using Application.ErrorMessages;                 // BusinessRuleError
using Application.OutBoundPort.IAssetRepository; // IAddAssetAsync
using Application.OutBoundPort.IFileStorage;     // ISaveFileAsync
using Application.UseCases.Assets;               // UploadAssetUseCase
using Domain.Entities;                           // Asset
using Domain.Enums;                              // AssetKind
using Moq;

namespace MoqTests.UseCases.Assets
{
    public class UploadAssetMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<ISaveFileAsync> saveFile = new();
        private readonly Mock<IAddAssetAsync> addAsset = new();
        private readonly Mock<TimeProvider> time = new();

        public UploadAssetMoqTests()
        {
            // xUnit creates a new instance of this class for every test, so this runs before each one.
            time.Setup(t => t.GetUtcNow()).Returns(AssetTestData.Now);
            saveFile.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync("stored-xyz");
        }

        private UploadAssetUseCase CreateUseCase() => new(saveFile.Object, addAsset.Object, time.Object);

        private static UploadAssetCommand Command(string contentType = "image/png", long size = 3, string name = "cat.png")
            => new(Stream.Null, name, contentType, size);

        [Fact]
        public async Task Upload_saves_file_then_adds_asset_with_the_stored_name()
        {
            Asset? saved = null;
            addAsset.Setup(a => a.AddAssetAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()))
                    .Callback<Asset, CancellationToken>((asset, _) => saved = asset)
                    .Returns(Task.CompletedTask);

            var result = await CreateUseCase().UploadAsync(Command(), Ct);

            Assert.True(result.IsSuccess);
            Assert.NotNull(saved);
            Assert.Same(result.Value, saved);
            Assert.Equal(AssetKind.Image, saved.Kind);
            Assert.Equal("cat.png", saved.OriginalName);
            Assert.Equal("stored-xyz", saved.StoredName);
            Assert.Equal("image/png", saved.ContentType);
            Assert.Equal(3L, saved.SizeBytes);
            Assert.Equal(AssetTestData.Now, saved.UploadedAt);
        }

        // All 10 allowed types from the spec (Accept header on GetAsset in Output.cs), written out by hand,
        // so removing one from Asset.KindFromContentType makes this test fail.
        [Theory]
        [InlineData("image/png", AssetKind.Image)]
        [InlineData("image/jpeg", AssetKind.Image)]
        [InlineData("image/webp", AssetKind.Image)]
        [InlineData("image/gif", AssetKind.Image)]
        [InlineData("video/mp4", AssetKind.Video)]
        [InlineData("video/webm", AssetKind.Video)]
        [InlineData("video/x-matroska", AssetKind.Video)]
        [InlineData("audio/mpeg", AssetKind.Audio)]
        [InlineData("audio/wav", AssetKind.Audio)]
        [InlineData("audio/mp4", AssetKind.Audio)]
        public async Task Upload_picks_kind_from_content_type(string contentType, AssetKind expected)
        {
            var result = await CreateUseCase().UploadAsync(Command(contentType), Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(expected, result.Value.Kind);
        }

        [Fact]
        public async Task Upload_with_unknown_content_type_fails_and_saves_nothing()
        {
            var result = await CreateUseCase().UploadAsync(Command("application/pdf"), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Contains(result.Errors, e => e.Message == "File type not allowed.");
            saveFile.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            addAsset.Verify(a => a.AddAssetAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // Limits from the spec (Output.cs), written out on purpose. Don't use Asset.MaxBytesFor here:
        // then the test would agree with the code even if the code had the wrong limit.
        [Theory]
        [InlineData("image/png", 10_000_000L)]
        [InlineData("audio/mpeg", 50_000_000L)]
        [InlineData("video/mp4", 500_000_000L)]
        public async Task Upload_at_max_is_allowed_and_one_byte_over_fails(string contentType, long maxBytes)
        {
            var atMax = await CreateUseCase().UploadAsync(Command(contentType, maxBytes), Ct);
            var overMax = await CreateUseCase().UploadAsync(Command(contentType, maxBytes + 1), Ct);

            Assert.True(atMax.IsSuccess);
            Assert.Equal(maxBytes, atMax.Value.SizeBytes);
            Assert.True(overMax.HasError<BusinessRuleError>());
            Assert.Contains(overMax.Errors, e => e.Message == "File is too large.");
        }

        [Fact]
        public async Task Upload_too_large_saves_nothing()
        {
            var result = await CreateUseCase().UploadAsync(Command("image/png", 10_000_001L), Ct);

            Assert.True(result.IsFailed);
            saveFile.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            addAsset.Verify(a => a.AddAssetAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // StartsWith, because ArgumentException adds " (Parameter 'originalName')" to the message.
        [Theory]
        [InlineData("", 3L, "Name required.")]        // no name
        [InlineData("   ", 3L, "Name required.")]     // whitespace name
        [InlineData("cat.png", 0L, "File is empty.")] // empty file
        public async Task Upload_with_invalid_data_is_BusinessRuleError_and_adds_no_row(string name, long size, string expectedMessage)
        {
            var result = await CreateUseCase().UploadAsync(Command("image/png", size, name), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Contains(result.Errors, e => e.Message.StartsWith(expectedMessage));
            addAsset.Verify(a => a.AddAssetAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // Spec: a failed upload must not leave a file on disk, so SaveFileAsync must not run
        // when the name or size is invalid.
        [Fact]
        public async Task Upload_that_fails_validation_leaves_no_file_behind()
        {
            var result = await CreateUseCase().UploadAsync(Command("image/png", 3L, ""), Ct);

            Assert.True(result.IsFailed);
            saveFile.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}