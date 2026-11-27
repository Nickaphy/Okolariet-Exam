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
                throw new GatewayException(response.StatusCode);
            return response.Content;
        }

        public static void EnsureSuccess(this IApiResponse response)
        {
            if (!response.IsSuccessStatusCode)
                throw new GatewayException(response.StatusCode);
        }
    }
}
