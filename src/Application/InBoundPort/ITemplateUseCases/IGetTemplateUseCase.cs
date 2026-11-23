using Domain.Entities;   // Template
using FluentResults;     // Result

namespace Application.InBoundPort.ITemplateUseCases
{
    public interface IGetTemplateUseCase
    {
        Task<Result<Template>> GetAsync(string templateType, CancellationToken ct);
    }
}