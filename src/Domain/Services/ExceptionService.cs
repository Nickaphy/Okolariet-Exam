namespace Domain.Services
{
    namespace WatchWorld.Domain.Service
    {
        // Handmade exceptions, used in try catches to filter through some of the most common exception. Also making it possible to distinct between ->
        // simple dumb user errors we don't want to crash the system, or vital SQL errors that could be a sign of a bigger problem.
        public class DomainException(string message) : Exception(message);

        public sealed class NotFoundException(string message) : DomainException(message);

        public sealed class UserInvalidInputException(string message) : DomainException(message);

        public sealed class ValidationException(string message) : DomainException(message);
    }

}
