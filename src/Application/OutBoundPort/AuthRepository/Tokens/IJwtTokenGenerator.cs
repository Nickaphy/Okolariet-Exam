using Domain.Entities;

namespace Application.OutBoundPort.AuthRepository.Tokens
{
    public interface IJwtTokenGenerator
    {
        string Generate(StaffMember staff, Guid loginId, DateTimeOffset expiresAt);
    }
}
