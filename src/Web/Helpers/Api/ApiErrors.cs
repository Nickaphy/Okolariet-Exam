using System.Net;
using Refit;

namespace Web.Api;

// Turns API failures into short Danish sentences staff can act on.
// The API's ProblemDetails are in English and meant for developers, so the UI
// maps on status code instead of showing <c>detail</c> directly.
public static class ApiErrors
{
    public const string Offline =
        "Der er ingen forbindelse til serveren. Tjek internettet, og prøv igen.";

    // What dialogs and pages want after a call: null when it worked, otherwise a message. overrides replace the generic message for one status, e.g. (Conflict, "E-mailen bruges allerede.").
    public static string? ErrorOrNull(IApiResponse response, bool ignoreNotFound = false,
        params (HttpStatusCode Status, string Message)[] overrides)
    {
        if (response.IsSuccessStatusCode || (ignoreNotFound && response.StatusCode == HttpStatusCode.NotFound))     // ignoreNotFound treats 404 as success (someone else already deleted it).
            return null;

        foreach (var (status, message) in overrides)
        {
            if (response.StatusCode == status) return message;
        }

        return Describe(response);
    }

    public static string Describe(IApiResponse response) => response.StatusCode switch // Mapped and translated API errors
    {
        HttpStatusCode.Unauthorized => "Du er blevet logget ud. Log ind igen.", //401
        HttpStatusCode.Forbidden => "Du har ikke adgang til at gøre dette.", //403
        HttpStatusCode.NotFound => "Det findes ikke længere. Måske har en anden slettet det.", //404
        HttpStatusCode.Conflict => "Det kan ikke lade sig gøre, fordi det stadig bliver brugt.", //409
        HttpStatusCode.PreconditionFailed => "En anden har foretaget ændringer. Hent siden igen, og prøv igen.", //412
        HttpStatusCode.UnprocessableEntity => "Det er ikke tilladt her.", //422
        HttpStatusCode.TooManyRequests => "For mange forsøg. Vent lidt, og prøv igen.", //429
        HttpStatusCode.BadRequest => "Noget af det udfyldte er ikke gyldigt.", //400
        HttpStatusCode.RequestEntityTooLarge => "Filen er for stor.", //413
        >= HttpStatusCode.InternalServerError => "Der skete en fejl på serveren. Prøv igen om lidt.", //500
        _ => "Noget gik galt. Prøv igen."
    };
}
