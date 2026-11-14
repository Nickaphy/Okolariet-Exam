using FluentResults;

namespace Application.ErrorMessages
{ // 409 Conlict http error. Used mostly for Update (put) calls. 
    public class ConflictError(string message) : Error(message);
}
