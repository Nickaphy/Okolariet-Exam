using Application.InBoundPort.ITemplateUseCases;     // IListTemplatesUseCase
using Application.OutBoundPort.ITemplateRepository;  // IListTemplatesAsync
using Domain.Entities;                               // Template
using FluentResults;                                 // Result

namespace Application.UseCases.Templates
{
    public class ListTemplatesUseCase(IListTemplatesAsync listTemplates) : IListTemplatesUseCase
    {
        public async Task<Result<IReadOnlyList<Template>>> ListAsync(CancellationToken ct)
        {
            var templates = await listTemplates.ListTemplatesAsync(ct);
            return Result.Ok(templates);
        }
    }
}