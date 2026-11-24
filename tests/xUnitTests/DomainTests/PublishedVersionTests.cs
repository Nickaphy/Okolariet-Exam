using Domain.Entities;       // PublishedVersion
using Domain.ValueObjects;   // VersionSnapshot, Appearance

namespace xUnitTests.DomainTests
{
    public class PublishedVersionTests
    {
        private static readonly VersionSnapshot Snapshot = new(Appearance.Empty, Guid.NewGuid(), []);

        [Fact]
        public void Create_sets_all_values()
        {
            var projectId = Guid.NewGuid();
            var publishedAt = new DateTimeOffset(2026, 11, 8, 10, 15, 0, TimeSpan.Zero);

            var version = PublishedVersion.Create(projectId, 1, publishedAt, Snapshot);

            Assert.Equal(projectId, version.ProjectId);
            Assert.Equal(1, version.VersionNumber);
            Assert.Equal(publishedAt, version.PublishedAt);
            Assert.Same(Snapshot, version.Snapshot);
            Assert.NotEqual(Guid.Empty, version.Id);
        }

        [Fact]
        public void Create_without_project_throws()
        {
            Assert.Throws<ArgumentException>(() => PublishedVersion.Create(Guid.Empty, 1, DateTimeOffset.UnixEpoch, Snapshot));
        }

        // 0 is reserved for the draft preview, which is never saved.
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_with_version_number_below_1_throws(int versionNumber)
        {
            Assert.Throws<ArgumentException>(() => PublishedVersion.Create(Guid.NewGuid(), versionNumber, DateTimeOffset.UnixEpoch, Snapshot));
        }

        [Fact]
        public void Create_without_snapshot_throws()
        {
            Assert.Throws<ArgumentNullException>(() => PublishedVersion.Create(Guid.NewGuid(), 1, DateTimeOffset.UnixEpoch, null!));
        }
    }
}
