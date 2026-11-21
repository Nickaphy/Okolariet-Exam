using Application.OutBoundPort.ITemplateRepository;   // IGetTemplateByTypeAsync, IListTemplatesAsync
using Domain.Entities;                                // Template

namespace xUnitTests.ApplicationTests.Fakes
{
    // Stand-in for CodeTemplateCatalog. Starts with the menu and quiz test templates.
    public class FakeTemplateCatalog : IGetTemplateByTypeAsync, IListTemplatesAsync
    {
        public List<Template> Templates { get; } = [TestData.MenuTemplate(), TestData.QuizTemplate()];

        public Task<Template?> GetTemplateByTypeAsync(string templateType, CancellationToken ct)
            => Task.FromResult(Templates.FirstOrDefault(t => t.Type == templateType));

        public Task<IReadOnlyList<Template>> ListTemplatesAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<Template>>(Templates.ToList());
    }
}
