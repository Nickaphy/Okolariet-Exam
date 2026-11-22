using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Requests.NicknameRequests
{
    // Represents the nickname selection request with the first word ID, second word ID, and a number.
    public record NicknameSelectionRequest(
    Guid FirstWordId,
    Guid SecondWordId,
    int Number);
}