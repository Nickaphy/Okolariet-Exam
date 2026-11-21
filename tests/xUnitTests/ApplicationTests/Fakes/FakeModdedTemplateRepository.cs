using Application.OutBoundPort.IModdedTemplateRepository;   // the modded template ports
using Domain.Entities;                                      // ModdedTemplate

namespace xUnitTests.ApplicationTests.Fakes
{
    public class FakeModdedTemplateRepository :
        IGetModdedTemplateByIdAsync, IListModdedTemplatesByProjectAsync, IAddModdedTemplateAsync,
        IUpdateModdedTemplateAsync, IDeleteModdedTemplateAsync, IIsLinkedFromButtonAsync, ITemplateSupportsQuestionsAsync
    {
        public List<ModdedTemplate> ModdedTemplates { get; } = new();
        public HashSet<Guid> SupportsQuestions { get; } = new();   // ids of modded templates built from a quiz template
        public int UpdateCount { get; private set; }

        public Task<ModdedTemplate?> GetModdedTemplateByIdAsync(Guid moddedTemplateId, CancellationToken ct)
            => Task.FromResult(ModdedTemplates.FirstOrDefault(m => m.Id == moddedTemplateId));

        public Task<IReadOnlyList<ModdedTemplate>> ListModdedTemplatesByProjectAsync(Guid projectId, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ModdedTemplate>>(ModdedTemplates.Where(m => m.ProjectId == projectId).ToList());

        public Task AddModdedTemplateAsync(ModdedTemplate moddedTemplate, CancellationToken ct)
        {
            ModdedTemplates.Add(moddedTemplate);
            return Task.CompletedTask;
        }

        public Task UpdateModdedTemplateAsync(ModdedTemplate moddedTemplate, CancellationToken ct)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task DeleteModdedTemplateAsync(ModdedTemplate moddedTemplate, CancellationToken ct)
        {
            ModdedTemplates.Remove(moddedTemplate);
            return Task.CompletedTask;
        }

        // Works like the real query will: does a button on ANOTHER modded template point here?
        public Task<bool> IsLinkedFromButtonAsync(Guid moddedTemplateId, CancellationToken ct)
            => Task.FromResult(ModdedTemplates.Any(m =>
                m.Id != moddedTemplateId && m.Items.Any(i => i.TargetModdedTemplateId == moddedTemplateId)));

        public Task<bool> SupportsQuestionsAsync(Guid moddedTemplateId, CancellationToken ct)
            => Task.FromResult(SupportsQuestions.Contains(moddedTemplateId));
    }
}
