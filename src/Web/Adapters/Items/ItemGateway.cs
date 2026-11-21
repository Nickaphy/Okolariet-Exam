using Web.Api;             // IItemsApi
using Web.Api.Contracts;   // ModdedTemplateItem, ModdedTemplateItemInput
using Web.Helpers;         // .Unwrap(), .EnsureSuccess()
using Web.Ports.Items;     // IItemGateway

namespace Web.Adapters.Items
{
    public class ItemGateway(IItemsApi api) : IItemGateway
    {
        public async Task<ModdedTemplateItem> SetItem(Guid moddedTemplateId, string slotKey, ModdedTemplateItemInput body, CancellationToken cancellationToken = default)
            => (await api.SetItem(moddedTemplateId, slotKey, body, cancellationToken)).Unwrap();

        public async Task ClearItem(Guid moddedTemplateId, string slotKey, CancellationToken cancellationToken = default)
            => (await api.ClearItem(moddedTemplateId, slotKey, cancellationToken)).EnsureSuccess();
    }
}