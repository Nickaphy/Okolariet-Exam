using Refit;
using Web.Exceptions;

namespace Web.Helpers
{
    // Helper method since most API's return IApiResponse<T> and we want to throw a GatewayException if the response is not successful or the content is null.
    // Helps us become more DRY compliant.
    public static class ApiResponseExceptionHelper
    {
        public static T Unwrap<T>(this IApiResponse<T> res)
        {
            if (!res.IsSuccessStatusCode || res.Content is null)
                throw new GatewayException(res.StatusCode); //Sends any response message that is not successful to the GatewayException, which then seperates them based off responsetype.
            return res.Content;
        }

        public static void EnsureSuccess(this IApiResponse res) //This method is used if the IApiResponse doesn't wrap anything (no content -> Missing the <T>). Typically happens on Deletes
        {
            if (!res.IsSuccessStatusCode)
                throw new GatewayException(res.StatusCode);
        }
    }
}
