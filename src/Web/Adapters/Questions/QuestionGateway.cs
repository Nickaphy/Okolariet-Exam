using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Ports.Questions;

//In here, if the API sends something back, it will unwrap; if it sends nothing, it will delete it and ensure it did (EnsureSuccess).
namespace Web.Adapters.Questions
{
    public class QuestionGateway(IQuestionsApi api) : IQuestionGateway
    {
        public async Task<IReadOnlyList<Question>> ListQuestions(Guid moddedTemplateId, CancellationToken cancellationToken = default)
            => (await api.ListQuestions(moddedTemplateId, cancellationToken)).Unwrap().ToList();

        public async Task<Question> CreateQuestion(Guid moddedTemplateId, QuestionInput body, CancellationToken cancellationToken = default)
            => (await api.CreateQuestion(moddedTemplateId, body, cancellationToken)).Unwrap();

        public async Task<Question> GetQuestion(Guid questionId, CancellationToken cancellationToken = default)
            => (await api.GetQuestion(questionId, cancellationToken)).Unwrap();

        public async Task<Question> UpdateQuestion(Guid questionId, QuestionInput body, string ifMatch, CancellationToken cancellationToken = default)
            => (await api.UpdateQuestion(questionId, body, ifMatch, cancellationToken)).Unwrap();

        public async Task DeleteQuestion(Guid questionId, CancellationToken cancellationToken = default)
            => (await api.DeleteQuestion(questionId, cancellationToken)).EnsureSuccess();
    }
}