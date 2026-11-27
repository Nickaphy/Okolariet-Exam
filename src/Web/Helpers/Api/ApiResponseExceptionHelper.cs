using Refit;
using Web.Exceptions;

namespace Web.Helpers
{
    // Used inside the gateways (Adapters/...) only. Refit hands back an IApiResponse instead of throwing,
    // so every gateway method ends with one of these two:
    //   .Unwrap()        the call returns data:    give back the data, or throw GatewayException on an error status;
    //   .EnsureSuccess() the call returns nothing: throw GatewayException on an error status (typically deletes).
    public static class ApiResponseExceptionHelper
    {
        public static T Unwrap<T>(this IApiResponse<T> response)
        {
            if (!response.IsSuccessStatusCode || response.Content is null)
                throw Failure(response);
            return response.Content;
        }

        public static void EnsureSuccess(this IApiResponse response)
        {
            if (!response.IsSuccessStatusCode)
                throw Failure(response);
        }

        // The status code, and the Retry-After header if the server sent one (e.g. "wait 15 minutes" after 5 wrong passwords).
        private static GatewayException Failure(IApiResponse response) =>
            new(response.StatusCode ?? System.Net.HttpStatusCode.InternalServerError, response.Headers?.RetryAfter?.Delta);
    }
}
