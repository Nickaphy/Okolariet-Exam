using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database
{
    // Value converters that store a complex object as a single JSON text column.
    // Used for Appearance and the published snapshot. Both are always read and written as a whole and are never
    // queried by their content, so a JSON column is sufficient
    internal static class JsonConversions
    {
        // Enums are written as text instead of numbers, so the stored JSON stays readable
        // and does not break if the order of an enum's values changes later.
        private static readonly JsonSerializerOptions Options = new()
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public static readonly ValueConverter<Appearance, string> AppearanceConverter =
            new(v => ToJson(v), v => AppearanceFromJson(v));

        public static readonly ValueConverter<VersionSnapshot, string> SnapshotConverter =
            new(v => ToJson(v), v => SnapshotFromJson(v));

        private static string ToJson<T>(T value) => JsonSerializer.Serialize(value, Options);

        // A missing or empty value falls back to Appearance.Empty (use the defaults).
        private static Appearance AppearanceFromJson(string json) =>
            JsonSerializer.Deserialize<Appearance>(json, Options) ?? Appearance.Empty;

        // A published snapshot is immutable and always written in full, so it is never null in the database.
        private static VersionSnapshot SnapshotFromJson(string json) =>
            JsonSerializer.Deserialize<VersionSnapshot>(json, Options)!;
    }
}