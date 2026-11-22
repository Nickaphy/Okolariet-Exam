using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Models
{
    // GET /api/v1/sessions/{sessionId}
    // Carries the session's progress and project-wide comparison
    // from the Application use case back to the controller.
    public record SessionSummary(
        Guid Id,
        DateTime StartedAt,
        int TotalQuestions,
        int Answered,
        int Correct,
        double Percentage,
        double AllGuestsAveragePercentage);
}