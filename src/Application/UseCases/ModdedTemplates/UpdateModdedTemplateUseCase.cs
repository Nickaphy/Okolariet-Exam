using Application.Commands.ModdedTemplateCommands;          // UpdateModdedTemplateCommand
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError
using Application.InBoundPort.IModdedTemplateUseCases;      // IUpdateModdedTemplateUseCase
using Application.OutBoundPort.IModdedTemplateRepository;   // IGetModdedTemplateByIdAsync, IUpdateModdedTemplateAsync
using Domain.Entities;                                      // ModdedTemplate
using FluentResults;                                        // Result

namespace Application.UseCases.ModdedTemplates
{
    public class UpdateModdedTemplateUseCase(
        IGetModdedTemplateByIdAsync getModdedTemplate,
        IUpdateModdedTemplateAsync updateModdedTemplate) : IUpdateModdedTemplateUseCase
    {
        public async Task<Result<ModdedTemplate>> UpdateAsync(UpdateModdedTemplateCommand cmd, CancellationToken ct)
        {
            try
            {
                var moddedTemplate = await getModdedTemplate.GetModdedTemplateByIdAsync(cmd.ModdedTemplateId, ct);
                if (moddedTemplate is null)
                    return Result.Fail(new NotFoundError("Modded template not found."));
                    // 404

                // TODO: check cmd.IfMatch against the RowVersion once Infrastructure has it (same as UpdateQuestion)

                moddedTemplate.Update(cmd.Title, cmd.Appearance?.ToDomain(), cmd.SortOrder);
                await updateModdedTemplate.UpdateModdedTemplateAsync(moddedTemplate, ct);
                return Result.Ok(moddedTemplate);
            }
            catch (ArgumentException ex)
            {
                return Result.Fail(new BusinessRuleError(ex.Message));
                // 422
            }
        }
    }
}