namespace xUnitTests.ApplicationTests.Fakes
{
    // A clock that always says the same time, so UploadedAt can be checked exactly.
    public class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
