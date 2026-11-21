using Domain.Entities;   // Template
using FluentResults;     // Result

namespace Application.InBoundPort.ITemplateUseCases
{
    public interface IListTemplatesUseCase
    {
        Task<Result<IReadOnlyList<Template>>> ListAsync(CancellationToken ct);
    }
}