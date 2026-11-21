using Application.Helpers;   // ModdedTemplateWithQuestions
using FluentResults;         // Result

namespace Application.InBoundPort.IModdedTemplateUseCases
{
    public interface IGetModdedTemplateUseCase
    {
        Task<Result<ModdedTemplateWithQuestions>> GetAsync(Guid moddedTemplateId, CancellationToken ct);
    }
}