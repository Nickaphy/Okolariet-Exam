using FluentResults;

namespace Application.ErrorMessages
{ // Error is basically -> 404 not found
    public class NotFoundError(string message) : Error(message);
}
