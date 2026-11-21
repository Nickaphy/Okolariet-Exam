using Domain.Entities;   // ModdedTemplateItem

namespace Application.Helpers
{
    // SetItem must tell the controller whether the item is NEW (201 Created) or REPLACED (200 OK).
    public record SetItemResult(ModdedTemplateItem Item, bool Created);
}