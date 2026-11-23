using Application.Commands.ModdedTemplateCommands;          // CreateModdedTemplateCommand
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError
using Application.InBoundPort.IModdedTemplateUseCases;      // ICreateModdedTemplateUseCase
using Application.OutBoundPort.IModdedTemplateRepository;   // IAddModdedTemplateAsync
using Application.OutBoundPort.IProjectRepository;          // IProjectExistsAsync
using Application.OutBoundPort.ITemplateRepository;         // IGetTemplateByTypeAsync (yours, from Template)
using Domain.Entities;                                      // ModdedTemplate
using FluentResults;                                        // Result

namespace Application.UseCases.ModdedTemplates
{
    public class CreateModdedTemplateUseCase(
        IProjectExistsAsync projectExists,
        IGetTemplateByTypeAsync getTemplate,
        IAddModdedTemplateAsync addModdedTemplate) : ICreateModdedTemplateUseCase
    {
        public async Task<Result<ModdedTemplate>> CreateAsync(CreateModdedTemplateCommand cmd, CancellationToken ct)
        {
            try
            {
                if (!await projectExists.ProjectExistsAsync(cmd.ProjectId, ct))
                    return Result.Fail(new NotFoundError("Project not found."));
                    // 404

                var template = await getTemplate.GetTemplateByTypeAsync(cmd.TemplateType, ct);
                if (template is null)
                    return Result.Fail(new BusinessRuleError($"Unknown template type '{cmd.TemplateType}'."));
                    // 422

                var moddedTemplate = ModdedTemplate.Create(cmd.ProjectId, template, cmd.Title, cmd.Appearance?.ToDomain(), cmd.SortOrder);
                await addModdedTemplate.AddModdedTemplateAsync(moddedTemplate, ct);
                return Result.Ok(moddedTemplate);
            }
            catch (ArgumentException ex)   // bad title or bad colour, thrown by the domain
            {
                return Result.Fail(new BusinessRuleError(ex.Message));
                // 422
            }
        }
    }
}