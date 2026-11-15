using Application.ErrorMessages;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Helpers
{
    // Helper method that translates a FluentResults exception into a readable StatusCode. Helpful for logging and user feedback.
    public static class HTTPResultMapperHelper
    {
        public static ActionResult ToProblem(this ControllerBase controller, IResultBase result)
        {
            var error = result.Errors.FirstOrDefault();
            var (status, title) = error switch
            {
                NotFoundError => (StatusCodes.Status404NotFound, "Not Found"),
                ConflictError => (StatusCodes.Status409Conflict, "Conflict"),
                TooManyRequestsError => (StatusCodes.Status429TooManyRequests, "Too Many Requests"),
                UnauthorizedError => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                BusinessRuleError => (StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity"),
                UnexpectedError => (StatusCodes.Status500InternalServerError, "Controlled Internal Server Error"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            // never leak internal error text for unexpected failures
            var detail = status == 500 ? "An unexpected error occurred." : error?.Message;
            return controller.Problem(detail: detail, statusCode: status, title: title);
        }
    }
}
