using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Commands.UserCommands
{
   public record CreateUserCommand(
    Guid SessionId,
    Guid ModdedTemplateId,
    Guid FirstWordId,
    Guid SecondWordId,
    int Number);
}