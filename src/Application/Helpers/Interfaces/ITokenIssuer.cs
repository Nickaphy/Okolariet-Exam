using Application.Auth;
using Domain.Entities;
using Domain.Entities.AuthEntities;

namespace Application.Helpers.Interfaces
{
    public interface ITokenIssuer
    {
        Task<AuthTokens> IssueAsync(StaffMember staff, StaffLogin login, CancellationToken ct = default);
    }
}
