using Web.Api.Contracts;


// Gateway implementation for Questions. Takes Output.cs methods and converts/connects them to Ports.
namespace Web.Ports.Questions
{
    public interface IQuestionGateway
    {
        Task<IReadOnlyList<Question>> ListQuestions(Guid moddedTemplateId, CancellationToken cancellationToken = default);
        Task<Question> CreateQuestion(Guid moddedTemplateId, QuestionInput body, CancellationToken cancellationToken = default);
        Task<Question> GetQuestion(Guid questionId, CancellationToken cancellationToken = default);
        Task<Question> UpdateQuestion(Guid questionId, QuestionInput body, string ifMatch, CancellationToken cancellationToken = default);
        Task DeleteQuestion(Guid questionId, CancellationToken cancellationToken = default);
    }
}