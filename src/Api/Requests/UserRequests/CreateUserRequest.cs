using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NicknameSelectionRequest = Api.Requests.NicknameRequests.NicknameSelectionRequest;

namespace Api.Requests.UserRequests
{
    // Represents the request to create a new user with a session ID, modded template ID, and nickname selection.
   public record CreateUserRequest(Guid SessionId, Guid ModdedTemplateId, NicknameSelectionRequest Nickname); 
}