using Domain.ValueObjects;

namespace Domain.Entities.AuthEntities
{
    public class StaffLogin : Entity // Staff login token, used to track refresh tokens and allow revocation of refresh tokens
    {
        public Guid StaffId { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }   // CreatedAt + 8h, absolute
        public DateTimeOffset? RevokedAt { get; set; }

        public bool IsActive(DateTimeOffset now) => RevokedAt is null && now < ExpiresAt;
    }
}
