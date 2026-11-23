using Domain.Entities;   // Asset
using Domain.Enums;      // AssetKind, SlotKind

namespace xUnitTests.DomainTests
{
    public class AssetTests
    {
        [Fact]
        public void Create_sets_all_properties()
        {
            var now = new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero);

            var asset = Asset.Create(AssetKind.Image, "cat.png", "abc.png", "image/png", 1234, now);

            Assert.NotEqual(Guid.Empty, asset.Id);
            Assert.Equal(AssetKind.Image, asset.Kind);
            Assert.Equal("cat.png", asset.OriginalName);
            Assert.Equal("abc.png", asset.StoredName);
            Assert.Equal("image/png", asset.ContentType);
            Assert.Equal(1234, asset.SizeBytes);
            Assert.Equal(now, asset.UploadedAt);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void Create_without_name_throws(string name)
        {
            Assert.Throws<ArgumentException>(() =>
                Asset.Create(AssetKind.Image, name, "abc.png", "image/png", 10, DateTimeOffset.UnixEpoch));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_with_empty_file_throws(long size)
        {
            Assert.Throws<ArgumentException>(() =>
                Asset.Create(AssetKind.Image, "cat.png", "abc.png", "image/png", size, DateTimeOffset.UnixEpoch));
        }

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
        public void KindFromContentType_maps_allowed_types(string contentType, AssetKind expected)
        {
            Assert.Equal(expected, Asset.KindFromContentType(contentType));
        }

        [Theory]
        [InlineData("application/pdf")]
        [InlineData("text/html")]
        [InlineData("")]
        public void KindFromContentType_returns_null_for_unknown_types(string contentType)
        {
            Assert.Null(Asset.KindFromContentType(contentType));
        }

        [Theory]
        [InlineData(AssetKind.Image, 10_000_000)]
        [InlineData(AssetKind.Audio, 50_000_000)]
        [InlineData(AssetKind.Video, 500_000_000)]
        public void MaxBytesFor_matches_the_limits_in_the_contract(AssetKind kind, long expected)
        {
            Assert.Equal(expected, Asset.MaxBytesFor(kind));
        }

        [Theory]
        [InlineData(AssetKind.Image, SlotKind.Image, true)]
        [InlineData(AssetKind.Video, SlotKind.Video, true)]
        [InlineData(AssetKind.Audio, SlotKind.Audio, true)]
        [InlineData(AssetKind.Audio, SlotKind.Image, false)]
        [InlineData(AssetKind.Image, SlotKind.Video, false)]
        [InlineData(AssetKind.Video, SlotKind.Audio, false)]
        [InlineData(AssetKind.Image, SlotKind.Text, false)]
        [InlineData(AssetKind.Image, SlotKind.Button, false)]
        public void FitsSlot_only_when_kinds_match(AssetKind assetKind, SlotKind slotKind, bool expected)
        {
            var asset = TestData.Asset(assetKind);

            Assert.Equal(expected, asset.FitsSlot(slotKind));
        }
    }
}
