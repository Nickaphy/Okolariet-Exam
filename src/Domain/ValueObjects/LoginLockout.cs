namespace Domain.ValueObjects
{
    public class LoginLockout // Used if a user has attempted to login too many times with the wrong password. 
    {
        public string Email { get; init; } = "";          // normalised i.e. lowercased, acts as primary key
        public int FailedAttempts { get; set; }
        public DateTimeOffset? LockedUntil { get; set; } // 15 minutes
    }
}
