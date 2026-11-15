using FluentResults;

namespace Application.ErrorMessages
{
    public class UnauthorizedError(string message) : Error(message);
}
