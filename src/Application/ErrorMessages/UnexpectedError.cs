using FluentResults;

namespace Application.ErrorMessages
{
    // This error message is for catch all exceptions we do not expect. This will typically be SQL or port related exceptions.
    // We do not want to expose these to the user, but we want to log them for debugging purposes.
    // Should be used carefully, as it will catch all exceptions, and not let the system crash. If a critical error is hit, it can be better to let a system crash.
    public class UnexpectedError(string message) : Error(message);
}
