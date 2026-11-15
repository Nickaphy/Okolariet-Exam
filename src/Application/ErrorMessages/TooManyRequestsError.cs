using FluentResults;

namespace Application.ErrorMessages
{
    public class TooManyRequestsError(string message, int retryAfterSeconds) : Error(message)
    {
        public int RetryAfterSeconds { get; } = retryAfterSeconds;
    }
}
