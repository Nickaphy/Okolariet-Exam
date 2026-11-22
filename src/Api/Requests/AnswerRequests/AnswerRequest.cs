using System.ComponentModel.DataAnnotations;

namespace Api.Requests.AnswerRequests
{
    public record AnswerRequest(
    [property: Required, StringLength(200, MinimumLength = 1)] string Text,
    Guid? AssetId,
    bool IsCorrect,
    int SortOrder);
}