using Application.ErrorMessages;                       // NotFoundError, BusinessRuleError, UnexpectedError
using Application.OutBoundPort.IProjectRepository;     // IGetProjectByIdAsync, IUpdateProjectAsync
using Application.OutBoundPort.IVersionRepository;     // IBuildSnapshotAsync, IGetLatestVersionAsync, IAddVersionAsync
using Application.UseCases.Versions;                   // PublishVersionUseCase
using Domain.Entities;                                 // Project, PublishedVersion
using Domain.ValueObjects;                             // VersionSnapshot, ModdedTemplateSnapshot, Appearance
using FluentResults;                                   // Result
using Moq;
using MoqTests.TestHelpers;                            // MockTime

namespace MoqTests.UseCases.Versions
{
    // Spec (contracts/openapi.yaml, POST /projects/{id}/versions): snapshots the project as a new immutable version.
    // Fails with 422 if the project is not publishable (for example a quiz question without exactly one correct answer).
    public class PublishVersionMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = MockTime.Now;

        private readonly Mock<IGetProjectByIdAsync> getProject = new();
        private readonly Mock<IBuildSnapshotAsync> buildSnapshot = new();
        private readonly Mock<IGetLatestVersionAsync> getLatest = new();
        private readonly Mock<IAddVersionAsync> add = new();
        private readonly Mock<IUpdateProjectAsync> updateProject = new();

        private readonly Project project = Project.Create("Havets Hus");
        private readonly VersionSnapshot snapshot = new(Appearance.Empty, Guid.NewGuid(), Array.Empty<ModdedTemplateSnapshot>());

        public PublishVersionMoqTests()
        {
            getProject.Setup(g => g.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);
            buildSnapshot.Setup(b => b.BuildAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Ok(snapshot));
        }

        private PublishVersionUseCase CreateUseCase() => new(
            getProject.Object, buildSnapshot.Object, getLatest.Object, add.Object, updateProject.Object, MockTime.At(Now));

        private void AssertNothingSaved()
        {
            add.Verify(a => a.AddAsync(It.IsAny<PublishedVersion>(), It.IsAny<CancellationToken>()), Times.Never);
            updateProject.Verify(u => u.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.Null(project.LatestVersionNumber);
        }

        [Fact]
        public async Task First_publish_is_version_1()
        {
            var result = await CreateUseCase().PublishAsync(project.Id, Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Value.VersionNumber);
            Assert.Equal(project.Id, result.Value.ProjectId);
            Assert.Equal(Now, result.Value.PublishedAt);
            Assert.Same(snapshot, result.Value.Snapshot);
            Assert.Equal(1, project.LatestVersionNumber);
            add.Verify(a => a.AddAsync(result.Value, It.IsAny<CancellationToken>()), Times.Once);
            updateProject.Verify(u => u.UpdateAsync(project, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Next_publish_is_the_latest_version_plus_one()
        {
            var latest = PublishedVersion.Create(project.Id, 3, Now.AddDays(-1), snapshot);
            getLatest.Setup(g => g.GetLatestAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(latest);

            var result = await CreateUseCase().PublishAsync(project.Id, Ct);

            Assert.Equal(4, result.Value.VersionNumber);
            Assert.Equal(4, project.LatestVersionNumber);
        }

        [Fact]
        public async Task Project_that_is_not_publishable_passes_the_error_on_and_saves_nothing()
        {
            buildSnapshot.Setup(b => b.BuildAsync(project.Id, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Result.Fail<VersionSnapshot>(new BusinessRuleError("A quiz question needs exactly one correct answer.")));

            var result = await CreateUseCase().PublishAsync(project.Id, Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Contains(result.Errors, e => e.Message == "A quiz question needs exactly one correct answer.");
            AssertNothingSaved();
        }

        [Fact]
        public async Task Unknown_project_is_NotFound()
        {
            var result = await CreateUseCase().PublishAsync(Guid.NewGuid(), Ct);

            Assert.True(result.HasError<NotFoundError>());
            buildSnapshot.Verify(b => b.BuildAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            AssertNothingSaved();
        }

        [Fact]
        public async Task Database_error_becomes_UnexpectedError_without_leaking_details()
        {
            add.Setup(a => a.AddAsync(It.IsAny<PublishedVersion>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("Server=db;Password=secret"));

            var result = await CreateUseCase().PublishAsync(project.Id, Ct);

            Assert.True(result.HasError<UnexpectedError>());
            Assert.DoesNotContain(result.Errors, e => e.Message.Contains("secret"));
        }
    }
}
