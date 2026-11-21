using Domain.Entities;   // ModdedTemplate
using FluentResults;     // Result

namespace Application.InBoundPort.IModdedTemplateUseCases
{
    public interface IListModdedTemplatesUseCase
    {
        Task<Result<IReadOnlyList<ModdedTemplate>>> ListAsync(Guid projectId, CancellationToken ct);
    }
}