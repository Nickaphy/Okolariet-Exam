using Application.ErrorMessages;                     // NotFoundError
using Application.InBoundPort.ITemplateUseCases;     // IGetTemplateUseCase
using Application.OutBoundPort.ITemplateRepository;  // IGetTemplateByTypeAsync
using Domain.Entities;                               // Template
using FluentResults;                                 // Result

namespace Application.UseCases.Templates
{
    public class GetTemplateUseCase(IGetTemplateByTypeAsync getTemplate) : IGetTemplateUseCase
    {
        public async Task<Result<Template>> GetAsync(string templateType, CancellationToken ct)
        {
            var template = await getTemplate.GetTemplateByTypeAsync(templateType, ct);
            if (template is null)
                return Result.Fail(new NotFoundError($"Template '{templateType}' not found."));   // 404

            return Result.Ok(template);
        }
    }
}