using Application.ErrorMessages;                            // NotFoundError
using Application.Helpers;                                  // ModdedTemplateWithQuestions
using Application.InBoundPort.IModdedTemplateUseCases;      // IGetModdedTemplateUseCase
using Application.OutBoundPort.IModdedTemplateRepository;   // IGetModdedTemplateByIdAsync
using Application.OutBoundPort.IQuestionRepository;         // IListQuestionsByTemplateAsync (yours, from Questions)
using FluentResults;                                        // Result

namespace Application.UseCases.ModdedTemplates
{
    public class GetModdedTemplateUseCase(
        IGetModdedTemplateByIdAsync getModdedTemplate,
        IListQuestionsByTemplateAsync listQuestions) : IGetModdedTemplateUseCase
    {
        public async Task<Result<ModdedTemplateWithQuestions>> GetAsync(Guid moddedTemplateId, CancellationToken ct)
        {
            var moddedTemplate = await getModdedTemplate.GetModdedTemplateByIdAsync(moddedTemplateId, ct);
            if (moddedTemplate is null)
                return Result.Fail(new NotFoundError("Modded template not found."));

            var questions = await listQuestions.ListQuestionsByTemplateAsync(moddedTemplateId, ct);
            var activeQuestions = questions.Where(q => q.IsActive).OrderBy(q => q.SortOrder).ToList();   
            // soft-deleted questions stay hidden

            return Result.Ok(new ModdedTemplateWithQuestions(moddedTemplate, activeQuestions));
        }
    }
}