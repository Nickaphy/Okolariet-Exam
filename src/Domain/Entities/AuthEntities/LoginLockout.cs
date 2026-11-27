namespace Domain.Entities.AuthEntities
{
    // Failed login attempts for one e-mail. After 5 failures the e-mail is locked for 15 minutes (see LoginUseCase).
    // It is stored and changed over time, with the e-mail as its key, so it is an entity (not a value object).
    public class LoginLockout
    {
        public string Email { get; init; } = "";          // normalised (lower-case), the primary key
        public int FailedAttempts { get; set; }
        public DateTimeOffset? LockedUntil { get; set; }  // set for 15 minutes after the 5th failure
    }
}
