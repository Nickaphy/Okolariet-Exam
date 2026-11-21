using Domain.ValueObjects;   // Entity

namespace Domain.Entities
{
    // The content of ONE slot in a modded template, e.g. "button1 says 'Quiz' and leads to modded template X".
    // An entity (not an aggregate root): it only exists inside its ModdedTemplate, the same way an Answer only exists inside a Question.
    public class ModdedTemplateItem : Entity
    {
        public Guid ModdedTemplateId { get; private set; }
        public string SlotKey { get; private set; } = "";
        public string? Text { get; private set; }
        public Guid? AssetId { get; private set; }
        public Guid? TargetModdedTemplateId { get; private set; }

        private ModdedTemplateItem() { } // EF Core

        // internal = only code in the Domain project can call it, so the ONLY way in is ModdedTemplate.SetItem.
        internal static ModdedTemplateItem Create(Guid moddedTemplateId, string slotKey, string? text, Guid? assetId, Guid? targetModdedTemplateId)
            => new()
            {
                ModdedTemplateId = moddedTemplateId,
                SlotKey = slotKey,
                Text = text,
                AssetId = assetId,
                TargetModdedTemplateId = targetModdedTemplateId
            };

        internal void Update(string? text, Guid? assetId, Guid? targetModdedTemplateId)
        {
            Text = text;
            AssetId = assetId;
            TargetModdedTemplateId = targetModdedTemplateId;
        }
    }
}