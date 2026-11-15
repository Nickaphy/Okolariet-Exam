using Domain.ValueObjects;

namespace Domain.Entities.AuthEntities
{
    public class RefreshToken : Entity // Refresh token entity, used to track refresh tokens and allow revocation of refresh tokens
    {
        public Guid LoginId { get; init; } // Id of staff member, not the token itself.
        public string TokenHash { get; init; } = "";      // never the raw token
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
        public DateTimeOffset? UsedAt { get; set; }      // set on rotation; reuse = UsedAt not null
    }
}
