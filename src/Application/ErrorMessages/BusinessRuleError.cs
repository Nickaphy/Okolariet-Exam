using FluentResults;

namespace Application.ErrorMessages
{ // 422 Unprocessable Entity http error. Quite a rare error, but it is used when a business rule is violated.
  // In this instance, it is used when the user tries to add a word that is already in the list of possible words.
    public class BusinessRuleError(string message) : Error(message);
}
