using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.Api;

/// <summary>
/// JSON settings shared by every Refit client.
/// </summary>
public static class ApiJson
{
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // The generated enums carry [EnumMember(Value = "refresh_token")], but
        // System.Text.Json ignores that attribute. A converter in Options.Converters
        // takes precedence over the [JsonConverter] on the enum type, so this makes
        // the wire values match the OpenAPI spec exactly ("admin", "refresh_token",
        // "bestScore") instead of relying on a naming policy that happens to fit.
        options.Converters.Add(new EnumMemberJsonConverterFactory());
        return options;
    }
}

/// <summary>Serialises enums using their [EnumMember] value (falls back to camelCase).</summary>
public sealed class EnumMemberJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(EnumMemberConverter<>).MakeGenericType(typeToConvert))!;

    private sealed class EnumMemberConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        private readonly Dictionary<TEnum, string> _toJson = new();
        private readonly Dictionary<string, TEnum> _fromJson = new(StringComparer.OrdinalIgnoreCase);

        public EnumMemberConverter()
        {
            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var value = (TEnum)field.GetValue(null)!;
                var wireName = field.GetCustomAttribute<EnumMemberAttribute>()?.Value
                               ?? JsonNamingPolicy.CamelCase.ConvertName(field.Name);

                _toJson[value] = wireName;
                _fromJson[wireName] = value;
                _fromJson.TryAdd(field.Name, value);
            }
        }

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String
                && reader.GetString() is { } text
                && _fromJson.TryGetValue(text, out var value))
            {
                return value;
            }

            throw new JsonException($"Unknown value for {typeof(TEnum).Name}.");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
            writer.WriteStringValue(_toJson.TryGetValue(value, out var text) ? text : value.ToString());
    }
}
