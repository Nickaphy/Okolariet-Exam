using Application.OutBoundPort.IQuestionRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Questions
{
    public sealed class GetQuestionByIdAdapter(OkolarietDbContext db) : IGetQuestionByIdAsync
    {
        // Tracked on purpose: Update/Delete and the answer use cases change it and save it in the same request.
        public Task<Question?> GetQuestionByIdAsync(Guid questionId, CancellationToken ct) =>
            db.Questions.FirstOrDefaultAsync(q => q.Id == questionId, ct);
    }
}