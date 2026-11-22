using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Commands.QuizResultCommands
{
    // The command that defines what the Application recieves when a guest submits an answer
    public record SubmitQuizResultCommand(
        Guid SessionId,
        Guid QuestionId,
        Guid AnswerId
    );
}