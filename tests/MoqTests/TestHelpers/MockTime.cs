using Moq;

namespace MoqTests.TestHelpers
{
    // A TimeProvider that always says "it is now this time", so tests can check exact timestamps.
    internal static class MockTime
    {
        public static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

        public static TimeProvider At(DateTimeOffset now)
        {
            var time = new Mock<TimeProvider>();
            time.Setup(t => t.GetUtcNow()).Returns(now);
            return time.Object;
        }
    }
}
