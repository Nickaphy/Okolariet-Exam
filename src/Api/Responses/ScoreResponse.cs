using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Responses
{
    // Response object for score-related API responses
    public record ScoreResponse(int Correct, int TotalQuestions, double Percentage);
}