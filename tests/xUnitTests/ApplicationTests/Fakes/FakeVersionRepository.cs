using Application.OutBoundPort.IVersionRepository;   // the version ports
using Domain.Entities;                               // PublishedVersion
using Domain.ValueObjects;                           // VersionSnapshot, Appearance
using FluentResults;                                 // Result

namespace xUnitTests.ApplicationTests.Fakes
{
    public class FakeVersionRepository : IAddVersionAsync, IListVersionsAsync, IGetLatestVersionAsync, IBuildSnapshotAsync
    {
        public List<PublishedVersion> Versions { get; } = new();

        // What BuildAsync returns. Set it to a failed Result to simulate a project that can't be published yet.
        public Result<VersionSnapshot> SnapshotResult { get; set; } = Result.Ok(EmptySnapshot());

        public bool FailOnSave { get; set; }   // simulates the database being unavailable

        public static VersionSnapshot EmptySnapshot() => new(Appearance.Empty, Guid.NewGuid(), []);

        public Task AddAsync(PublishedVersion version, CancellationToken ct)
        {
            if (FailOnSave)
                throw new InvalidOperationException("Database unavailable.");
            Versions.Add(version);
            return Task.CompletedTask;
        }

        // Newest first, like the real adapter.
        public Task<IReadOnlyList<PublishedVersion>> ListAsync(Guid projectId, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<PublishedVersion>>(
                Versions.Where(v => v.ProjectId == projectId).OrderByDescending(v => v.VersionNumber).ToList());

        public Task<PublishedVersion?> GetLatestAsync(Guid projectId, CancellationToken ct)
            => Task.FromResult(Versions.Where(v => v.ProjectId == projectId).OrderByDescending(v => v.VersionNumber).FirstOrDefault());

        public Task<Result<VersionSnapshot>> BuildAsync(Guid projectId, CancellationToken ct)
            => Task.FromResult(SnapshotResult);
    }
}
