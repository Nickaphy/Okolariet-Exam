using Application.OutBoundPort.IQuestionRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Questions
{
    public sealed class UpdateQuestionAdapter(OkolarietDbContext db) : IUpdateQuestionAsync
    {
        // NO db.Update(): same reason as UpdateModdedTemplateAdapter. A new Answer from AddAnswer
        // already has an Id, so Update() would try to UPDATE it instead of INSERT.
        public Task UpdateQuestionAsync(Question question, CancellationToken ct) =>
            db.SaveChangesAsync(ct);
    }
}