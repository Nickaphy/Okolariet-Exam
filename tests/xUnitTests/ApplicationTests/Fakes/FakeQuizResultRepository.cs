using Application.OutBoundPort.IModdedTemplateRepository;
using Application.OutBoundPort.IQuizResultRepository;
using Domain.Entities;

namespace xUnitTests.ApplicationTests.Fakes
{
    public class FakeQuizResultRepository :
        ITryAddQuizResultAsync, IListQuizResultsBySessionAsync, ITemplateBelongsToProjectAsync
    {
        public List<QuizResult> Results { get; } = new();
        public Guid QuizId { get; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }

        public Task<bool> BelongsToProjectAsync(Guid moddedTemplateId, Guid projectId, CancellationToken ct)
            => Task.FromResult(moddedTemplateId == QuizId && projectId == ProjectId);

        public Task<bool> TryAddAsync(QuizResult result, CancellationToken ct)
        {
            if (Results.Any(saved => saved.SessionId == result.SessionId && saved.QuestionId == result.QuestionId))
                return Task.FromResult(false);

            Results.Add(result);
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<QuizResult>> ListBySessionAsync(Guid sessionId, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<QuizResult>>(
                Results.Where(result => result.SessionId == sessionId).ToList());
    }
}
