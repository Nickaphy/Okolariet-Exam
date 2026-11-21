using Application.OutBoundPort.IQuestionRepository;   // the question ports
using Domain.Entities;                                // Question

namespace xUnitTests.ApplicationTests.Fakes
{
    public class FakeQuestionRepository :
        IAddQuestionAsync, IGetQuestionByIdAsync, IGetQuestionByAnswerIdAsync, IListQuestionsByTemplateAsync, IUpdateQuestionAsync
    {
        public List<Question> Questions { get; } = new();
        public int UpdateCount { get; private set; }   // lets a test check that the use case actually saved

        public Task AddQuestionAsync(Question question, CancellationToken ct)
        {
            Questions.Add(question);
            return Task.CompletedTask;
        }

        public Task<Question?> GetQuestionByIdAsync(Guid questionId, CancellationToken ct)
            => Task.FromResult(Questions.FirstOrDefault(q => q.Id == questionId));

        public Task<Question?> GetQuestionByAnswerIdAsync(Guid answerId, CancellationToken ct)
            => Task.FromResult(Questions.FirstOrDefault(q => q.Answers.Any(a => a.Id == answerId)));

        public Task<IReadOnlyList<Question>> ListQuestionsByTemplateAsync(Guid moddedTemplateId, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<Question>>(Questions.Where(q => q.ModdedTemplateId == moddedTemplateId).ToList());

        public Task UpdateQuestionAsync(Question question, CancellationToken ct)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }
    }
}
