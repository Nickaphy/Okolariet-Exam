using Domain.Enums;   // SlotKind

namespace Domain.ValueObjects
{
    // A "slot" is a fixed spot in a template that staff can fill in
    // Key is the slot's technical name, like "button1"
    // //Label is what staff see in the editor, like "Knap 1"
    public record SlotDefinition(string Key, string Label, SlotKind Kind, bool Required = false, int? MaxLength = null);
}