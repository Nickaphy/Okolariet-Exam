using System.Net;
using Refit;

namespace Web.Api;

/// <summary>
/// Turns API failures into short Danish sentences staff can act on.
/// The API's ProblemDetails are in English and meant for developers, so the UI
/// maps on status code instead of showing <c>detail</c> directly.
/// </summary>
public static class ApiErrors
{
    public const string Offline =
        "Der er ingen forbindelse til serveren. Tjek internettet, og prøv igen.";

    public static string Describe(IApiResponse response) => response.StatusCode switch
    {
        HttpStatusCode.Unauthorized => "Du er blevet logget ud. Log ind igen.",
        HttpStatusCode.Forbidden => "Du har ikke adgang til at gøre dette.",
        HttpStatusCode.NotFound => "Det findes ikke længere. Måske har en anden slettet det.",
        HttpStatusCode.Conflict => "Det kan ikke lade sig gøre, fordi det stadig bliver brugt.",
        HttpStatusCode.PreconditionFailed => "En anden har ændret det imens. Hent siden igen, og prøv igen.",
        HttpStatusCode.UnprocessableEntity => "Det er ikke tilladt her.",
        HttpStatusCode.TooManyRequests => "For mange forsøg. Vent lidt, og prøv igen.",
        HttpStatusCode.BadRequest => "Noget af det udfyldte er ikke gyldigt.",
        >= HttpStatusCode.InternalServerError => "Der skete en fejl på serveren. Prøv igen om lidt.",
        _ => "Noget gik galt. Prøv igen."
    };
}
