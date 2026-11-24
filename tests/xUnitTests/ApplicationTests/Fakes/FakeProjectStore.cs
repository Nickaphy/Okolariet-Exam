using Application.Helpers;                           // PagedList
using Application.OutBoundPort.IProjectRepository;   // the project ports
using Domain.Entities;                               // Project

namespace xUnitTests.ApplicationTests.Fakes
{
    // In-memory stand-in for the Project adapters used by the Project and Version use cases.
    // FakeProjectRepository is separate: it covers the two project ports the ModdedTemplate tests use.
    public class FakeProjectStore :
        IAddProjectAsync, IGetProjectByIdAsync, IListProjectsAsync, IUpdateProjectAsync, IDeleteProjectAsync,
        IModdedTemplateBelongsToProjectAsync
    {
        public List<Project> Projects { get; } = new();
        public HashSet<(Guid ModdedTemplateId, Guid ProjectId)> Screens { get; } = new();   // which screen belongs to which project
        public int UpdateCount { get; private set; }
        public bool FailOnSave { get; set; }   // simulates the database being unavailable

        public Task AddAsync(Project project, CancellationToken ct)
        {
            ThrowIfFailing();
            Projects.Add(project);
            return Task.CompletedTask;
        }

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct)
            => Task.FromResult(Projects.FirstOrDefault(p => p.Id == id));

        public Task<PagedList<Project>> ListAsync(int page, int pageSize, CancellationToken ct)
        {
            var items = Projects.OrderBy(p => p.Name).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(new PagedList<Project>(items, page, pageSize, Projects.Count));
        }

        public Task UpdateAsync(Project project, CancellationToken ct)
        {
            ThrowIfFailing();
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Project project, CancellationToken ct)
        {
            ThrowIfFailing();
            Projects.Remove(project);
            return Task.CompletedTask;
        }

        public Task<bool> BelongsToProjectAsync(Guid moddedTemplateId, Guid projectId, CancellationToken ct)
            => Task.FromResult(Screens.Contains((moddedTemplateId, projectId)));

        private void ThrowIfFailing()
        {
            if (FailOnSave)
                throw new InvalidOperationException("Database unavailable.");
        }
    }
}
