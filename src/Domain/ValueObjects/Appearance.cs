using System.Text.RegularExpressions;   // Regex

namespace Domain.ValueObjects
{
    // Colours and font of a modded template. Presentation only. null means "use the default".
    public record Appearance
    {
        public static readonly Appearance Empty = new(null, null, null, null);

        public string? BackgroundColor { get; }
        public string? ButtonColor { get; }
        public string? TextColor { get; }
        public string? FontFamily { get; }

        public Appearance(string? backgroundColor, string? buttonColor, string? textColor, string? fontFamily)
        {
            if (fontFamily is not null && string.IsNullOrWhiteSpace(fontFamily))
                throw new ArgumentException("Font family cannot be empty.", nameof(fontFamily));

            BackgroundColor = ValidateColor(backgroundColor, nameof(backgroundColor));
            ButtonColor = ValidateColor(buttonColor, nameof(buttonColor));
            TextColor = ValidateColor(textColor, nameof(textColor));
            FontFamily = fontFamily;
        }

        // A colour must look like #82afbb - # followed by 6 hex digits.
        private static string? ValidateColor(string? color, string paramName)
        {
            if (color is null)
                return null;
            if (!Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$"))
                throw new ArgumentException($"'{color}' does not follow colour code structure.", paramName);
            return color;
        }
    }
}