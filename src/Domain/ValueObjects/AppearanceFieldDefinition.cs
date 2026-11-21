using Domain.Enums;   // AppearanceFieldKind

namespace Domain.ValueObjects
{
    // A colour or font staff can change. 
    // AllowedValues = null means "anything goes"
    public record AppearanceFieldDefinition(string Key, string Label, AppearanceFieldKind Kind, IReadOnlyList<string>? AllowedValues = null);
}