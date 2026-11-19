using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Responses.QuizResultResponses
{
    public record QuizResultResponse(
        Guid QuestionId,
        Guid AnswerId,
        bool IsCorrect,
        Guid CorrectAnswerId
    );
}