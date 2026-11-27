using System.Globalization;

namespace Web.Helpers;

// Danish-looking numbers and dates for the UI. Written by hand (comma instead of dot) instead of
// using the da-DK culture, because Blazor WebAssembly may run without full culture data.
public static class Display
{
    // 70 -> "70 %", 27.3 -> "27,3 %"
    public static string Percent(double value) =>
        value.ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',') + " %";

    // The same percentage as a CSS width: always a dot, clamped to 0-100.
    public static string CssWidth(double percent) =>
        Math.Clamp(percent, 0, 100).ToString("0.#", CultureInfo.InvariantCulture) + "%";

    public static string FileSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:0} KB",
        < 1024L * 1024 * 1024 => $"{bytes / 1024.0 / 1024.0:0.#} MB".Replace('.', ','),
        _ => $"{bytes / 1024.0 / 1024.0 / 1024.0:0.#} GB".Replace('.', ',')
    };

    public static string Date(DateTimeOffset value) =>
        value.ToLocalTime().ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}