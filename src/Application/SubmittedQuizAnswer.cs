using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application
{
    // The record that carries the user submission, and the actual correct answer to the question.
   public record SubmittedQuizAnswer(
    QuizResult RecordResult,
    Guid CorrectAnswerId
   );
}