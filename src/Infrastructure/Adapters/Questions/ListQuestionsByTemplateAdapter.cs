using Application.OutBoundPort.IQuestionRepository;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Questions
{
    public sealed class ListQuestionsByTemplateAdapter(OkolarietDbContext db) : IListQuestionsByTemplateAsync
    {
        // Returns inactive questions too: the use cases filter on IsActive themselves.
        public async Task<IReadOnlyList<Question>> ListQuestionsByTemplateAsync(Guid moddedTemplateId, CancellationToken ct) =>
            await db.Questions
                .AsNoTracking()
                .Where(q => q.ModdedTemplateId == moddedTemplateId)
                .OrderBy(q => q.SortOrder)
                .ToListAsync(ct);
    }
}