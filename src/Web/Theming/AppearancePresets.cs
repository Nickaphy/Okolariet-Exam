using Web.Api.Contracts;

namespace Web.Theming
{
    public sealed record AppearancePreset(string Key, string Name, Appearance Appearance);

    // Ready-made themes, inspired by some of the different areas and subjects that Økolariet touch on. 
    // Themes -> Hav, Genbrug and Skov
    public static class AppearancePresets
    {
        public static readonly IReadOnlyList<AppearancePreset> All =
        [
            // Nothing chosen: falls back to Theme.Default.
            new("standard", "Standard", new Appearance()),

            new("hav", "Hav", new Appearance
            {
                BackgroundColor = "#0A6E8A", ButtonColor = "#F2C14E", TextColor = "#FFFFFF"
            }),

            new("skov", "Skov", new Appearance
            {
                BackgroundColor = "#F3EBD8", ButtonColor = "#5E7D3A", TextColor = "#2F3E1F"
            }),

            // The exam case: waste and recycling.
            new("genbrug", "Genbrug", new Appearance
            {
                BackgroundColor = "#263238", ButtonColor = "#7CB342", TextColor = "#FFFFFF"
            }),
        ];
    }
}