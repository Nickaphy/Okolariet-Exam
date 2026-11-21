using System.ComponentModel.DataAnnotations;   // [Required], [StringLength]

namespace Api.Requests.ModdedTemplateRequests
{
    public record CreateModdedTemplateRequest(
        [property: Required] string TemplateType,
        [property: Required, StringLength(120, MinimumLength = 1)] string Title,
        AppearanceRequest? Appearance,
        int SortOrder);
}