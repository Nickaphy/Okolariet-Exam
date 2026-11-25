using Application.ErrorMessages;            // NotFoundError, BusinessRuleError, UnexpectedError
using Application.UseCases.Versions;        // the version use cases
using Domain.Entities;                      // Project, PublishedVersion
using Domain.ValueObjects;                  // VersionSnapshot
using FluentResults;                        // Result
using xUnitTests.ApplicationTests.Fakes;    // FakeProjectStore, FakeVersionRepository, FixedTimeProvider

namespace xUnitTests.ApplicationTests.UseCases
{
    public class VersionUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

        private readonly FakeProjectStore projects = new();
        private readonly FakeVersionRepository versions = new();
        private readonly Project project = Project.Create("Havet");

        public VersionUseCaseTests()
        {
            projects.Projects.Add(project);   // xUnit makes a new instance per test, so every test starts with one project
        }

        private PublishVersionUseCase PublishUseCase() =>
            new(projects, versions, versions, versions, projects, new FixedTimeProvider(Now));

        private PublishedVersion AddVersion(Guid projectId, int versionNumber)
        {
            var version = PublishedVersion.Create(projectId, versionNumber, Now, FakeVersionRepository.EmptySnapshot());
            versions.Versions.Add(version);
            return version;
        }

        // Publish

        [Fact]
        public async Task First_publish_is_version_1()
        {
            var result = await PublishUseCase().PublishAsync(project.Id, Ct);

            Assert.Equal(1, result.Value.VersionNumber);
            Assert.Equal(Now, result.Value.PublishedAt);
            Assert.Single(versions.Versions);
        }

        [Fact]
        public async Task Publish_registers_the_new_version_on_the_project()
        {
            await PublishUseCase().PublishAsync(project.Id, Ct);

            Assert.Equal(1, project.LatestVersionNumber);
            Assert.Equal(1, projects.UpdateCount);
        }

        [Fact]
        public async Task Next_publish_continues_from_the_latest_version()
        {
            AddVersion(project.Id, 1);
            AddVersion(project.Id, 2);

            var result = await PublishUseCase().PublishAsync(project.Id, Ct);

            Assert.Equal(3, result.Value.VersionNumber);
        }

        [Fact]
        public async Task Version_numbers_are_counted_per_project()
        {
            AddVersion(Guid.NewGuid(), 5);   // another project's versions must not affect this one

            var result = await PublishUseCase().PublishAsync(project.Id, Ct);

            Assert.Equal(1, result.Value.VersionNumber);
        }

        [Fact]
        public async Task Publish_saves_the_snapshot_that_was_built()
        {
            var snapshot = FakeVersionRepository.EmptySnapshot();
            versions.SnapshotResult = Result.Ok(snapshot);

            var result = await PublishUseCase().PublishAsync(project.Id, Ct);

            Assert.Same(snapshot, result.Value.Snapshot);
        }

        [Fact]
        public async Task Publish_when_project_cannot_be_published_returns_that_error_and_saves_nothing()
        {
            versions.SnapshotResult = Result.Fail<VersionSnapshot>(new BusinessRuleError("No start screen."));

            var result = await PublishUseCase().PublishAsync(project.Id, Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(versions.Versions);
            Assert.Null(project.LatestVersionNumber);
            Assert.Equal(0, projects.UpdateCount);
        }

        [Fact]
        public async Task Publish_unknown_project_is_NotFound()
        {
            var result = await PublishUseCase().PublishAsync(Guid.NewGuid(), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task Publish_when_saving_fails_is_UnexpectedError()
        {
            versions.FailOnSave = true;

            var result = await PublishUseCase().PublishAsync(project.Id, Ct);

            Assert.True(result.HasError<UnexpectedError>());
        }

        // Get latest

        [Fact]
        public async Task GetLatest_returns_the_newest_version()
        {
            AddVersion(project.Id, 1);
            var newest = AddVersion(project.Id, 2);

            var result = await new GetLatestVersionUseCase(versions).GetLatestAsync(project.Id, Ct);

            Assert.Same(newest, result.Value);
        }

        [Fact]
        public async Task GetLatest_when_never_published_is_NotFound()
        {
            var result = await new GetLatestVersionUseCase(versions).GetLatestAsync(project.Id, Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        // List

        [Fact]
        public async Task List_returns_only_this_projects_versions()
        {
            var own = AddVersion(project.Id, 1);
            AddVersion(Guid.NewGuid(), 1);

            var result = await new ListVersionsUseCase(projects, versions).ListAsync(project.Id, Ct);

            Assert.Equal(new[] { own }, result.Value);
        }

        [Fact]
        public async Task List_for_unknown_project_is_NotFound()
        {
            var result = await new ListVersionsUseCase(projects, versions).ListAsync(Guid.NewGuid(), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        // Draft

        [Fact]
        public async Task Draft_returns_the_built_snapshot_without_saving_a_version()
        {
            var snapshot = FakeVersionRepository.EmptySnapshot();
            versions.SnapshotResult = Result.Ok(snapshot);

            var result = await new GetDraftVersionUseCase(projects, versions).GetDraftAsync(project.Id, Ct);

            Assert.Same(snapshot, result.Value);
            Assert.Empty(versions.Versions);
        }

        [Fact]
        public async Task Draft_when_project_cannot_be_shown_returns_that_error()
        {
            versions.SnapshotResult = Result.Fail<VersionSnapshot>(new BusinessRuleError("No start screen."));

            var result = await new GetDraftVersionUseCase(projects, versions).GetDraftAsync(project.Id, Ct);

            Assert.True(result.HasError<BusinessRuleError>());
        }

        [Fact]
        public async Task Draft_for_unknown_project_is_NotFound()
        {
            var result = await new GetDraftVersionUseCase(projects, versions).GetDraftAsync(Guid.NewGuid(), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }
    }
}
