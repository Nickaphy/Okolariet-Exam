using Application.Helpers;                         // PagedList
using Application.OutBoundPort.IAssetRepository;   // the asset ports
using Domain.Entities;                             // Asset
using Domain.Enums;                                // AssetKind

namespace xUnitTests.ApplicationTests.Fakes
{
    // An in-memory stand-in for the real database. One class can implement all the small port interfaces.
    public class FakeAssetRepository :
        IAddAssetAsync, IAssetExistsAsync, IAssetInUseAsync, IDeleteAssetAsync, IGetAssetByIdAsync, IListAssetsAsync
    {
        public List<Asset> Assets { get; } = new();
        public HashSet<Guid> InUse { get; } = new();   // put an asset id here to pretend something uses it

        public Task AddAssetAsync(Asset asset, CancellationToken ct)
        {
            Assets.Add(asset);
            return Task.CompletedTask;
        }

        public Task<bool> AssetExistsAsync(Guid assetId, CancellationToken ct)
            => Task.FromResult(Assets.Any(a => a.Id == assetId));

        public Task<bool> AssetInUseAsync(Guid assetId, CancellationToken ct)
            => Task.FromResult(InUse.Contains(assetId));

        public Task DeleteAssetAsync(Asset asset, CancellationToken ct)
        {
            Assets.Remove(asset);
            return Task.CompletedTask;
        }

        public Task<Asset?> GetAssetByIdAsync(Guid assetId, CancellationToken ct)
            => Task.FromResult(Assets.FirstOrDefault(a => a.Id == assetId));

        public Task<PagedList<Asset>> ListAssetsAsync(AssetKind? kind, int page, int pageSize, CancellationToken ct)
        {
            var filtered = Assets.Where(a => kind is null || a.Kind == kind).ToList();
            var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(new PagedList<Asset>(items, page, pageSize, filtered.Count));
        }
    }
}
