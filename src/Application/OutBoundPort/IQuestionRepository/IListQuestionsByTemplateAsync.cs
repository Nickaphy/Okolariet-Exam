using Domain.Entities;

namespace Application.OutBoundPort.IQuestionRepository
{
    public interface IListQuestionsByTemplateAsync
    {
        Task<IReadOnlyList<Question>> ListQuestionsByTemplateAsync(Guid moddedTemplateId, CancellationToken ct);
    }
}