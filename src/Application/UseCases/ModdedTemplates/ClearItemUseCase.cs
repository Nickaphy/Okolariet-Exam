using Application.Commands.ModdedTemplateCommands;          // ClearItemCommand
using Application.ErrorMessages;                            // NotFoundError
using Application.InBoundPort.IModdedTemplateUseCases;      // IClearItemUseCase
using Application.OutBoundPort.IModdedTemplateRepository;   // IGetModdedTemplateByIdAsync, IUpdateModdedTemplateAsync
using FluentResults;                                        // Result

namespace Application.UseCases.ModdedTemplates
{
    public class ClearItemUseCase(
        IGetModdedTemplateByIdAsync getModdedTemplate,
        IUpdateModdedTemplateAsync updateModdedTemplate) : IClearItemUseCase
    {
        public async Task<Result> ClearItemAsync(ClearItemCommand cmd, CancellationToken ct)
        {
            var moddedTemplate = await getModdedTemplate.GetModdedTemplateByIdAsync(cmd.ModdedTemplateId, ct);
            if (moddedTemplate is null)
                return Result.Fail(new NotFoundError("Modded template not found."));
                // 404

            moddedTemplate.ClearItem(cmd.SlotKey);   
            // can't fail, so no try/catch (same reasoning as Deactivate)

            await updateModdedTemplate.UpdateModdedTemplateAsync(moddedTemplate, ct);
            return Result.Ok();
        }
    }
}