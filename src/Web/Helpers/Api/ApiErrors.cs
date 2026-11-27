using System.Net;
using Web.Exceptions;

namespace Web.Api;

// Turns a failed API call into a short Danish sentence staff (or guests) can act on.
// The API's own error texts are English and meant for developers, so the text is picked from the status code.
public static class ApiErrors
{
    public const string Offline =
        "Der er ingen forbindelse til serveren. Tjek internettet, og prøv igen.";

    // The message for an exception from a gateway call.
    //   GatewayException     → the API answered with an error status → text for that status (or an override);
    //   HttpRequestException → no answer at all → the "offline" text.
    // overrides: a better text for one status code, e.g. (HttpStatusCode.Conflict, "Ordet findes allerede.").
    public static string For(Exception exception, params (HttpStatusCode Status, string Message)[] overrides)
    {
        if (exception is not GatewayException gateway) return Offline;

        foreach (var (status, message) in overrides)
        {
            if (gateway.StatusCode == status) return message;
        }

        return Describe(gateway.StatusCode);
    }

    // The general text for each status code.
    public static string Describe(HttpStatusCode status) => status switch
    {
        HttpStatusCode.BadRequest => "Noget af det udfyldte er ikke gyldigt.",                                   // 400
        HttpStatusCode.Unauthorized => "Du er blevet logget ud. Log ind igen.",                                   // 401
        HttpStatusCode.Forbidden => "Du har ikke adgang til at gøre dette.",                                      // 403
        HttpStatusCode.NotFound => "Det findes ikke længere. Måske har en anden slettet det.",                    // 404
        HttpStatusCode.Conflict => "Det kan ikke lade sig gøre, fordi det stadig bliver brugt.",                  // 409
        HttpStatusCode.PreconditionFailed => "En anden har foretaget ændringer. Hent siden igen, og prøv igen.",  // 412 (ETag no longer matches)
        HttpStatusCode.RequestEntityTooLarge => "Filen er for stor.",                                             // 413
        HttpStatusCode.UnprocessableEntity => "Det er ikke tilladt her.",                                         // 422 (breaks a business rule)
        HttpStatusCode.TooManyRequests => "For mange forsøg. Vent lidt, og prøv igen.",                           // 429
        >= HttpStatusCode.InternalServerError => "Der skete en fejl på serveren. Prøv igen om lidt.",             // 5xx
        _ => "Noget gik galt. Prøv igen."
    };
}
