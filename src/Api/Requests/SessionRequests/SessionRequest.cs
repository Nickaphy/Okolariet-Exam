using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Requests.SessionRequests
{
    // Represents the JSON the client sends when it calls POST /api/v1/session. The client must provide the ProjectId to create a new session.
    public record SessionRequest(Guid ProjectId);
}