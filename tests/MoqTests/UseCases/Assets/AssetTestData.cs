using Domain.Entities;
using Domain.Enums;

namespace MoqTests.UseCases.Assets
{
    internal static class AssetTestData
    {
        public static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

        // A valid image asset. Asset.Create gives it a new random Id every time.
        public static Asset Image(string storedName = "stored-123.png")
            => Asset.Create(AssetKind.Image, "cat.png", storedName, "image/png", 1_000, Now);
    }
}