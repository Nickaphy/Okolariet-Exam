namespace Web.Exceptions
{
    using System.Net;

    // This exception is thrown when a gateway (API) call fails with a non-success status code. It contains the HTTP status code and an optional detail message.
    // It exists purely to prevent a user from seing the Refit and HTTP types in the Web layer.
    // This message is not inteded to be shown to the user, but rather to be logged for debugging purposes and to be converted to a user-friendly error message. Example:
    // 403 Forbidden -> "You do not have access to this resource at the moment, if you believe this is an error try to re-login. If it persists please contact support"

    public class GatewayException(HttpStatusCode? statusCode, string? detail = null)
        : Exception(Message(statusCode, detail))
    {
        public HttpStatusCode? StatusCode { get; } = statusCode;

        public static string Message(HttpStatusCode? statusCode, string? detail = null)
        {
            if (detail is null)
                return statusCode switch
                {
                    // 400 Status codes -> user erros
                    HttpStatusCode.BadRequest => "The API returned 400 Bad Request.",
                    HttpStatusCode.Unauthorized => "The API returned 401 Unauthorized.",
                    HttpStatusCode.Forbidden => "The API returned 403 Forbidden.",
                    HttpStatusCode.NotFound => "The API returned 404 Not Found.",
                    HttpStatusCode.RequestTimeout => "The API returned 408 Request Timeout.",
                    HttpStatusCode.Conflict => "The API returned 409 Conflict.",
                    HttpStatusCode.TooManyRequests => "The API returned 429 Too Many Requests.",

                    // 500 Status codes -> server errors
                    HttpStatusCode.InternalServerError => "The API returned 500 Internal Server Error.",
                    HttpStatusCode.NotImplemented => "The API returned 501 Not Implemented.",
                    HttpStatusCode.BadGateway => "The API returned 502 Bad Gateway.",
                    HttpStatusCode.ServiceUnavailable => "The API returned 503 Service Unavailable.",
                    HttpStatusCode.GatewayTimeout => "The API returned 504 Gateway Timeout.",
                    _ => $"The API returned with an unexpected error, please contact support if this problem persists",
                };
            else
                return detail;
        }
    }
}
