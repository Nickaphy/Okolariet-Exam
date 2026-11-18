using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Responses.UserResponse
{
    // Response object for user-related API responses
    public record UserResponse(Guid Id, string Nickname, Guid ModdedTemplateId, ScoreResponse Score);
}