using Domain.Enums;          // AppearanceFieldKind
using Domain.ValueObjects;   // AppearanceFieldDefinition

namespace Api.Responses.TemplateResponses
{
    public record AppearanceFieldDefinitionResponse(string Key, string Label, AppearanceFieldKind Kind, IReadOnlyList<string>? AllowedValues)
    {
        public static AppearanceFieldDefinitionResponse From(AppearanceFieldDefinition a) => new(a.Key, a.Label, a.Kind, a.AllowedValues);
    }
}