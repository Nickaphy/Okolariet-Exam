using Web.Api.Contracts;   // ModdedTemplateItem, ModdedTemplateItemInput

namespace Web.Ports.Items
{
    // Gateway contract for the slots of a modded template. Same methods as IItemsApi in Output.cs.
    public interface IItemGateway
    {
        Task<ModdedTemplateItem> SetItem(Guid moddedTemplateId, string slotKey, ModdedTemplateItemInput body, CancellationToken cancellationToken = default);
        Task ClearItem(Guid moddedTemplateId, string slotKey, CancellationToken cancellationToken = default);
    }
}