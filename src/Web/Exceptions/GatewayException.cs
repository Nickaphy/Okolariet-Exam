using System.Net;

namespace Web.Exceptions
{
    // Thrown by a gateway (Adapters/...) when the API answers with an error status (4xx/5xx).
    //
    // It exists so the pages never see Refit or HTTP types: a page only calls a gateway, and when something
    // goes wrong, ApiCall (Helpers/Api/ApiCall.cs) catches this exception and turns StatusCode into a Danish
    // message from ApiErrors. The English Message is only for developers (logs, the test pages).
    //
    // RetryAfter: how long the server asked us to wait, when it sent a Retry-After header (429 Too Many Requests).
    public class GatewayException(HttpStatusCode statusCode, TimeSpan? retryAfter = null)
        : Exception($"The API answered {(int)statusCode} {statusCode}.")
    {
        public HttpStatusCode StatusCode { get; } = statusCode;
        public TimeSpan? RetryAfter { get; } = retryAfter;
    }
}
