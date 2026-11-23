using Domain.Enums;          // SlotKind
using Domain.ValueObjects;   // Aggregateroot, Appearance

namespace Domain.Entities
{
    // A staff member's filled-in copy of a Template. Belongs to a Project and holds the content of each slot (items).
    public class ModdedTemplate : Aggregateroot
    {
        private readonly List<ModdedTemplateItem> items = new();

        public Guid ProjectId { get; private set; }
        public string TemplateType { get; private set; } = "";   // which Template it's built from, e.g. "menu-ocean". Never changes.
        public string Title { get; private set; } = "";
        public Appearance Appearance { get; private set; } = Appearance.Empty;
        public int SortOrder { get; private set; }
        public IReadOnlyList<ModdedTemplateItem> Items => items;

        private ModdedTemplate() { } // EF Core

        // Takes the whole Template (not just a string), so you can't create a modded template from a type that doesn't exist.
        public static ModdedTemplate Create(Guid projectId, Template template, string title, Appearance? appearance, int sortOrder)
        {
            ValidateTitle(title);
            return new ModdedTemplate
            {
                ProjectId = projectId,
                TemplateType = template.Type,
                Title = title,
                Appearance = appearance ?? Appearance.Empty,
                SortOrder = sortOrder
            };
        }

        // No templateType parameter as Output.cs says the template type cannot be changed after creation.
        public void Update(string title, Appearance? appearance, int sortOrder)
        {
            ValidateTitle(title);
            Title = title;
            Appearance = appearance ?? Appearance.Empty;
            SortOrder = sortOrder;
        }

        public ModdedTemplateItem? FindItem(string slotKey) => items.FirstOrDefault(i => i.SlotKey == slotKey);

        // Creates or replaces the content of one slot.
        public ModdedTemplateItem SetItem(Template template, string slotKey, string? text, Asset? asset, Guid? targetModdedTemplateId)
        {
            if (template.Type != TemplateType)
                throw new ArgumentException("This is not the template the modded template was built from.", nameof(template));

            var slot = template.FindSlot(slotKey)
                ?? throw new ArgumentException($"The template has no slot called '{slotKey}'.", nameof(slotKey));

            if (text is not null && slot.MaxLength is int maxLength && text.Length > maxLength)
                throw new ArgumentException($"The text in '{slotKey}' can be at most {maxLength} characters.", nameof(text));

            if (asset is not null && !asset.FitsSlot(slot.Kind))
                throw new ArgumentException($"A {asset.Kind} file cannot go in a {slot.Kind} slot.", nameof(asset));

            if (targetModdedTemplateId is not null && slot.Kind != SlotKind.Button)
                throw new ArgumentException("Only buttons can lead to another modded template.", nameof(targetModdedTemplateId));

            if (targetModdedTemplateId == Id)
                throw new ArgumentException("A button cannot lead to its own modded template.", nameof(targetModdedTemplateId));

            var item = FindItem(slotKey);
            if (item is null)
            {
                item = ModdedTemplateItem.Create(Id, slotKey, text, asset?.Id, targetModdedTemplateId);
                items.Add(item);
            }
            else
            {
                item.Update(text, asset?.Id, targetModdedTemplateId);
            }
            return item;
        }

        // Clearing an empty slot is fine: the slot ends up empty either way.
        public void ClearItem(string slotKey)
        {
            var item = FindItem(slotKey);
            if (item is not null)
                items.Remove(item);
        }

        private static void ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length > 120)
                throw new ArgumentException("Title must be 1-120 characters.", nameof(title));
        }
    }
}