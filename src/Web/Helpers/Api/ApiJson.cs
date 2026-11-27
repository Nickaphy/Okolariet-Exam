using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.Api;

// JSON settings shared by every Refit client (see Program.cs) and by TokenStore.
public static class ApiJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        // Enums are sent as text in camelCase: StaffRole.Admin -> "admin", Sort.BestScore -> "bestScore",
        // TokenRequestGrantType.Refresh_token -> "refresh_token". That is exactly what openapi.yaml expects

        // Why it is needed: the generated enums carry [EnumMember(Value = "...")], but System.Text.Json
        // ignores that attribute, and would otherwise write "Admin". A converter added here wins over
        // the [JsonConverter] attribute the generator puts on each enum.
        //
        // If a new enum value in openapi.yaml is ever NOT camelCase of its C# name, this line must change.
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
