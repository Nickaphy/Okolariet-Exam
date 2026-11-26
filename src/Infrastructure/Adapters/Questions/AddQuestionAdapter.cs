using Application.OutBoundPort.IQuestionRepository;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Adapters.Questions
{
    public sealed class AddQuestionAdapter(OkolarietDbContext db) : IAddQuestionAsync
    {
        public async Task AddQuestionAsync(Question question, CancellationToken ct)
        {
            db.Questions.Add(question);   // its answers are added with it
            await db.SaveChangesAsync(ct);
        }
    }
}