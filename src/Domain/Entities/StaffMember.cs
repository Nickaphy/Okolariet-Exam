using Domain.Enums;

namespace Domain.Entities
{
    public class StaffMember
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";          // always stored lower-case
        public string PasswordHash { get; set; } = "";   // Hashed also means it must never leave the server
        public StaffRole Role { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
