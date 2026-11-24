using Application.ErrorMessages;
using Application.OutBoundPort.IVersionRepository;
using Domain.Enums;
using Domain.ValueObjects;
using FluentResults;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Versions
{
    // Builds a snapshot of the project as it looks right now. Used by Publish (saved) and the draft preview (not saved).
    // Answers are included without IsCorrect, so the kiosk can never reveal the right answer.
    public sealed class BuildSnapshotAdapter(OkolarietDbContext db) : IBuildSnapshotAsync
    {
        public async Task<Result<VersionSnapshot>> BuildAsync(Guid projectId, CancellationToken ct)
        {
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project is null)
                return Result.Fail<VersionSnapshot>(new NotFoundError("Project not found."));

            if (project.MainModdedTemplateId is not Guid startScreenId)
                return Result.Fail<VersionSnapshot>(new BusinessRuleError("Choose a start screen before previewing or publishing."));

            var screens = await db.ModdedTemplates
                .AsNoTracking()
                .Include(m => m.Items)
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);

            if (!screens.Any(s => s.Id == startScreenId))
                return Result.Fail<VersionSnapshot>(new BusinessRuleError("The start screen does not belong to this project."));

            // Deleted questions are soft-deleted, so only active ones are included.
            var screenIds = screens.Select(s => s.Id).ToList();
            var questions = await db.Questions
                .AsNoTracking()
                .Include(q => q.Answers)
                .Where(q => q.IsActive && screenIds.Contains(q.ModdedTemplateId))
                .OrderBy(q => q.SortOrder)
                .ToListAsync(ct);

            var invalid = questions.FirstOrDefault(q => !q.HasExactlyOneCorrectAnswer());
            if (invalid is not null)
                return Result.Fail<VersionSnapshot>(new BusinessRuleError($"The question '{invalid.Text}' must have exactly one correct answer."));

            // The kiosk needs to know whether each file is an image, video or audio. One query for all files.
            var assetIds = screens.SelectMany(s => s.Items).Select(i => i.AssetId)
                .Concat(questions.Select(q => q.AssetId))
                .Concat(questions.SelectMany(q => q.Answers).Select(a => a.AssetId))
                .OfType<Guid>()
                .Distinct()
                .ToList();

            var assetKinds = await db.Assets
                .AsNoTracking()
                .Where(a => assetIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a.Kind, ct);

            AssetKind? KindOf(Guid? assetId) =>
                assetId is Guid id && assetKinds.TryGetValue(id, out var kind) ? kind : null;

            var questionsByScreen = questions.ToLookup(q => q.ModdedTemplateId);

            var screenSnapshots = screens.Select(s => new ModdedTemplateSnapshot(
                s.Id,
                s.TemplateType,
                s.Title,
                s.Appearance,
                s.Items
                    .OrderBy(i => i.SlotKey)
                    .Select(i => new ItemSnapshot(i.SlotKey, i.Text, i.AssetId, KindOf(i.AssetId), i.TargetModdedTemplateId))
                    .ToList(),
                questionsByScreen[s.Id]
                    .Select(q => new QuestionSnapshot(
                        q.Id,
                        q.Text,
                        q.AssetId,
                        KindOf(q.AssetId),
                        q.Answers
                            .Where(a => a.IsActive)
                            .OrderBy(a => a.SortOrder)
                            .Select(a => new AnswerSnapshot(a.Id, a.Text, a.AssetId, KindOf(a.AssetId), a.SortOrder))
                            .ToList()))
                    .ToList()))
                .ToList();

            return Result.Ok(new VersionSnapshot(project.Appearance, startScreenId, screenSnapshots));
        }
    }
}
