using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Responses.SessionsResponses
{
// The JSON returned when the guest requests their session progress.
public record SessionSummaryResponse(
    Guid Id,
    DateTime StartedAt,
    int TotalQuestions,
    int Answered,
    int Correct,
    double Percentage,
    double AllGuestsAveragePercentage);
}