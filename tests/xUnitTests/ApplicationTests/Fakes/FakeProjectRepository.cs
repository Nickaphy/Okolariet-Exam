using Application.OutBoundPort.IProjectRepository;   // IProjectExistsAsync, IIsMainModdedTemplateAsync

namespace xUnitTests.ApplicationTests.Fakes
{
    // Project isn't built yet, so the tests just keep a list of "existing" project ids.
    public class FakeProjectRepository : IProjectExistsAsync, IIsMainModdedTemplateAsync
    {
        public HashSet<Guid> Projects { get; } = new();
        public HashSet<Guid> MainModdedTemplateIds { get; } = new();   // start screens

        public Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken ct)
            => Task.FromResult(Projects.Contains(projectId));

        public Task<bool> IsMainModdedTemplateAsync(Guid moddedTemplateId, CancellationToken ct)
            => Task.FromResult(MainModdedTemplateIds.Contains(moddedTemplateId));
    }
}
