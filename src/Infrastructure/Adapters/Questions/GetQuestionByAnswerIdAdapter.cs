using Application.OutBoundPort.IQuestionRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Questions
{
    public sealed class GetQuestionByAnswerIdAdapter(OkolarietDbContext db) : IGetQuestionByAnswerIdAsync
    {
        // Tracked on purpose, like GetQuestionByIdAdapter.
        public Task<Question?> GetQuestionByAnswerIdAsync(Guid answerId, CancellationToken ct) =>
            db.Questions.FirstOrDefaultAsync(q => q.Answers.Any(a => a.Id == answerId), ct);
    }
}