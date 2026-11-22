using System.ComponentModel.DataAnnotations;

namespace Api.Requests.QuestionRequests
{
    public record QuestionRequest(
    [property: Required, StringLength(500, MinimumLength = 1)] string Text,
    Guid? AssetId,
    int SortOrder);
}