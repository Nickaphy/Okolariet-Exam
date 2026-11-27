using System.Globalization;
using System.Text.RegularExpressions;
using Web.Api.Contracts;

namespace Web.Theming
{
    public sealed record FontOption(string Value, string Label);

    // How colours and font are worked out. The colours of a page are NEVER written in a template:
    // they come from the page's Appearance, else the project's Appearance, else the neutral default below.
    // These values are what ends up in the database (ModdedTemplate.Appearance / Project.Appearance).
    public static class Theme
    {
        // Used when nothing is chosen anywhere. Neutral on purpose; presets are shortcuts for choosing, not defaults.
        public static readonly Appearance Default = new()
        {
            BackgroundColor = "#F4F4F4",
            ButtonColor = "#2A2A2A",
            TextColor = "#2A2A2A"
        };

        // System fonts, so nothing has to be downloaded. "" = the app's normal font.
        public static readonly IReadOnlyList<FontOption> Fonts =
        [
            new("", "Standard"),
            new("Verdana", "Tydelig (Verdana)"),
            new("Trebuchet MS", "Rund (Trebuchet)"),
            new("Georgia", "Klassisk (Georgia)"),
        ];

        private static readonly Regex ColourPattern = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);
        private static readonly Regex ShortColourPattern = new("^#[0-9A-Fa-f]{3}$", RegexOptions.Compiled);
        private static readonly Regex AlphaColourPattern = new("^#[0-9A-Fa-f]{8}$", RegexOptions.Compiled);
        private static readonly Regex RgbPattern = new(@"^rgba?\(\s*([\d.]+)\s*,\s*([\d.]+)\s*,\s*([\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex FontPattern = new(@"^[\w -]{1,60}$", RegexOptions.Compiled);

        public static Appearance Resolve(Appearance? page, Appearance? project = null) => new()
        {
            BackgroundColor = PickColour(page?.BackgroundColor, project?.BackgroundColor, Default.BackgroundColor!),
            ButtonColor = PickColour(page?.ButtonColor, project?.ButtonColor, Default.ButtonColor!),
            TextColor = PickColour(page?.TextColor, project?.TextColor, Default.TextColor!),
            FontFamily = PickFont(page?.FontFamily) ?? PickFont(project?.FontFamily)
        };

        // CSS variables for a page (or a preview). Templates and previews only read these variables.
        // Everything is validated first, because the text ends up in a style attribute.
        public static string Style(Appearance? page, Appearance? project = null)
        {
            var a = Resolve(page, project);
            return $"--tpl-bg:{a.BackgroundColor};--tpl-button:{a.ButtonColor};" +
                   $"--tpl-button-ink:{ButtonInk(a.ButtonColor!)};--tpl-text:{a.TextColor};--tpl-font:{FontStack(a.FontFamily)}";
        }

        public static string FontStack(string? font) =>
            PickFont(font) is { } f ? $"\"{f}\", var(--font)" : "var(--font)";

        // Text on a button is black or white, whichever is easier to read on the button's colour.
        public static string ButtonInk(string buttonColour) =>
            Contrast("#FFFFFF", buttonColour) >= Contrast("#111111", buttonColour) ? "#FFFFFF" : "#111111";

        // WCAG contrast ratio, 1 to 21. 4.5 or more is comfortable for normal text.
        public static double Contrast(string a, string b)
        {
            var (hi, lo) = Luminance(a) >= Luminance(b) ? (Luminance(a), Luminance(b)) : (Luminance(b), Luminance(a));
            return (hi + 0.05) / (lo + 0.05);
        }

        public static bool IsColour(string? value) => value is not null && ColourPattern.IsMatch(value);

        // Turns what a colour picker gives back ("#abc", "#aabbcc", "#aabbccff", "rgb(1, 2, 3)", "rgba(...)")
        // into "#AABBCC". Returns null when it isn't a colour.
        public static string? ToHex(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var v = value.Trim();

            if (ColourPattern.IsMatch(v)) return v.ToUpperInvariant();
            if (AlphaColourPattern.IsMatch(v)) return v[..7].ToUpperInvariant();
            if (ShortColourPattern.IsMatch(v))
                return ("#" + string.Concat(v.Skip(1).Select(c => $"{c}{c}"))).ToUpperInvariant();

            var m = RgbPattern.Match(v);
            if (!m.Success) return null;

            static int Channel(string s) =>
                (int)Math.Clamp(Math.Round(double.Parse(s, CultureInfo.InvariantCulture)), 0, 255);

            return $"#{Channel(m.Groups[1].Value):X2}{Channel(m.Groups[2].Value):X2}{Channel(m.Groups[3].Value):X2}";
        }

        private static string PickColour(string? page, string? project, string fallback) =>
            IsColour(page) ? page! : IsColour(project) ? project! : fallback;

        private static string? PickFont(string? font) =>
            font is not null && FontPattern.IsMatch(font) ? font : null;

        private static double Luminance(string hex)
        {
            double Channel(int start)
            {
                var c = int.Parse(hex.AsSpan(start, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
                return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }
            return 0.2126 * Channel(1) + 0.7152 * Channel(3) + 0.0722 * Channel(5);
        }
    }
}
