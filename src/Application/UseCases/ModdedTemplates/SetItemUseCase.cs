using Application.Commands.ModdedTemplateCommands;          // SetItemCommand
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError, UnexpectedError
using Application.Helpers;                                  // SetItemResult
using Application.InBoundPort.IModdedTemplateUseCases;      // ISetItemUseCase
using Application.OutBoundPort.IAssetRepository;            // IGetAssetByIdAsync (yours, from Assets)
using Application.OutBoundPort.IModdedTemplateRepository;   // IGetModdedTemplateByIdAsync, IUpdateModdedTemplateAsync
using Application.OutBoundPort.ITemplateRepository;         // IGetTemplateByTypeAsync
using Domain.Entities;                                      // Asset
using FluentResults;                                        // Result

namespace Application.UseCases.ModdedTemplates
{
    public class SetItemUseCase(
        IGetModdedTemplateByIdAsync getModdedTemplate,
        IGetTemplateByTypeAsync getTemplate,
        IGetAssetByIdAsync getAsset,
        IUpdateModdedTemplateAsync updateModdedTemplate) : ISetItemUseCase
    {
        public async Task<Result<SetItemResult>> SetItemAsync(SetItemCommand cmd, CancellationToken ct)
        {
            try
            {
                // 1. The modded template we're filling in
                var moddedTemplate = await getModdedTemplate.GetModdedTemplateByIdAsync(cmd.ModdedTemplateId, ct);
                if (moddedTemplate is null)
                    return Result.Fail(new NotFoundError("Modded template not found."));
                    // 404

                // 2. Its blueprint, so the domain knows which slots exist
                var template = await getTemplate.GetTemplateByTypeAsync(moddedTemplate.TemplateType, ct);
                if (template is null)   // only happens if someone removed the template from the catalog
                    return Result.Fail(new UnexpectedError($"Template '{moddedTemplate.TemplateType}' no longer exists."));
                    // 500

                // 3. The file, if one was sent, so the domain can check that it fits the slot
                Asset? asset = null;
                if (cmd.AssetId is { } assetId)
                {
                    asset = await getAsset.GetAssetByIdAsync(assetId, ct);
                    if (asset is null)
                        return Result.Fail(new NotFoundError("Asset not found."));
                        // 404
                }

                // 4. The button's target, if one was sent: it must exist AND be in the same project
                if (cmd.TargetModdedTemplateId is { } targetId)
                {
                    var target = await getModdedTemplate.GetModdedTemplateByIdAsync(targetId, ct);
                    if (target is null || target.ProjectId != moddedTemplate.ProjectId)
                        return Result.Fail(new BusinessRuleError("A button must lead to a modded template in the same project."));
                        // 422
                }

                // 5. Ask BEFORE changing anything: is this slot empty right now? Then it's a create (201), otherwise a replace (200).
                var created = moddedTemplate.FindItem(cmd.SlotKey) is null;

                var item = moddedTemplate.SetItem(template, cmd.SlotKey, cmd.Text, asset, cmd.TargetModdedTemplateId);
                await updateModdedTemplate.UpdateModdedTemplateAsync(moddedTemplate, ct);
                // saves the item through its aggregate

                return Result.Ok(new SetItemResult(item, created));
            }
            catch (ArgumentException ex)   // wrong slot, text too long, wrong file kind... thrown by the domain
            {
                return Result.Fail(new BusinessRuleError(ex.Message));
                // 422
            }
        }
    }
}