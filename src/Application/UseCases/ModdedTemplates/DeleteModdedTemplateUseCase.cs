using Application.Commands.ModdedTemplateCommands;          // DeleteModdedTemplateCommand
using Application.ErrorMessages;                            // NotFoundError, ConflictError
using Application.InBoundPort.IModdedTemplateUseCases;      // IDeleteModdedTemplateUseCase
using Application.OutBoundPort.IModdedTemplateRepository;   // IGetModdedTemplateByIdAsync, IIsLinkedFromButtonAsync, IDeleteModdedTemplateAsync
using Application.OutBoundPort.IProjectRepository;          // IIsMainModdedTemplateAsync
using FluentResults;                                        // Result

namespace Application.UseCases.ModdedTemplates
{
    public class DeleteModdedTemplateUseCase(
        IGetModdedTemplateByIdAsync getModdedTemplate,
        IIsMainModdedTemplateAsync isMainModdedTemplate,
        IIsLinkedFromButtonAsync isLinkedFromButton,
        IDeleteModdedTemplateAsync deleteModdedTemplate) : IDeleteModdedTemplateUseCase
    {
        public async Task<Result> DeleteAsync(DeleteModdedTemplateCommand cmd, CancellationToken ct)
        {
            var moddedTemplate = await getModdedTemplate.GetModdedTemplateByIdAsync(cmd.ModdedTemplateId, ct);
            if (moddedTemplate is null)
                return Result.Fail(new NotFoundError("Modded template not found."));
                // 404

            // Both checks come from the remarks on DeleteModdedTemplate in Output.cs.
            // Without them the kiosk would have a start screen or a button that leads nowhere.
            if (await isMainModdedTemplate.IsMainModdedTemplateAsync(moddedTemplate.Id, ct))
                return Result.Fail(new ConflictError("This is the project's start screen. Choose another start screen first."));
                // 409

            if (await isLinkedFromButton.IsLinkedFromButtonAsync(moddedTemplate.Id, ct))
                return Result.Fail(new ConflictError("A button on another modded template leads here. Remove that link first."));
                // 409

            await deleteModdedTemplate.DeleteModdedTemplateAsync(moddedTemplate, ct);
            return Result.Ok();
        }
    }
}