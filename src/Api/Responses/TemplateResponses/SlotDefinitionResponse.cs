using Domain.Enums;          // SlotKind
using Domain.ValueObjects;   // SlotDefinition

namespace Api.Responses.TemplateResponses
{
    public record SlotDefinitionResponse(string Key, string Label, SlotKind Kind, bool Required, int? MaxLength)
    {
        public static SlotDefinitionResponse From(SlotDefinition s) => new(s.Key, s.Label, s.Kind, s.Required, s.MaxLength);
    }
}