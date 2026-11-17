using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Responses.SessionsResponses
{
    // represents what the API sends after creating a session
    public record SessionResponse(Guid Id, Guid ProjectId, DateTime StartedAt); 
}