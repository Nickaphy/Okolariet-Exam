using Application.ErrorMessages;           // NotFoundError
using Application.UseCases.Templates;      // GetTemplateUseCase, ListTemplatesUseCase
using xUnitTests.ApplicationTests.Fakes;   // FakeTemplateCatalog

namespace xUnitTests.ApplicationTests.UseCases
{
    public class TemplateUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly FakeTemplateCatalog catalog = new();

        [Fact]
        public async Task List_returns_every_template_in_the_catalog()
        {
            var result = await new ListTemplatesUseCase(catalog).ListAsync(Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task Get_known_type_returns_the_template()
        {
            var result = await new GetTemplateUseCase(catalog).GetAsync(TestData.QuizType, Ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(TestData.QuizType, result.Value.Type);
        }

        [Fact]
        public async Task Get_unknown_type_is_NotFound()
        {
            var result = await new GetTemplateUseCase(catalog).GetAsync("does-not-exist", Ct);

            Assert.True(result.HasError<NotFoundError>());
        }
    }
}
