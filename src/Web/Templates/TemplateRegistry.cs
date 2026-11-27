using Web.Templates.Menus;
using Web.Templates.Facts;
using Web.Templates.Menus.MenuTemplates;

namespace Web.Templates
{
    // Type   = the template type the server uses (TemplateCatalog in Infrastructure). Must match exactly.
    // Sample = example content for the template gallery (Admin → Skabeloner) and previews.
    public sealed record TemplateEntry(string Type, Type Component, string Name, Func<PageContent> Sample);

    // Which Blazor component draws which template. One line per template.
    // Written out by hand (no reflection), so trimming in WebAssembly can't remove a template by accident.
    public static class TemplateRegistry
    {
        public static readonly IReadOnlyList<TemplateEntry> All =
        [
            // Main Menu
            new("menu-buttons",   typeof(MenuButtons),   "Hovedmenu – 4 knapper i række",       MenuButtons.Sample),
            new("menu-split-4",   typeof(MenuSplitFour), "Hovedmenu – 4 knapper ved siden af",  MenuSplitFour.Sample),
            new("menu-three",     typeof(MenuThree),     "Hovedmenu – 3 store knapper",         MenuThree.Sample),
            new("menu-six",       typeof(MenuSix),       "Hovedmenu – 6 knapper",               MenuSix.Sample),
            new("menu-eight",     typeof(MenuEight),     "Hovedmenu – 8 knapper",               MenuEight.Sample),
            
            // Facts
            new("fact-image-text", typeof(FactImageText), "Fakta – billede og tekst",   FactImageText.Sample),
            new("fact-hero",       typeof(FactHero),      "Fakta – stort billede",      FactHero.Sample),
            new("fact-stats",      typeof(FactStats),     "Fakta – tre tal",            FactStats.Sample),
            new("fact-steps",      typeof(FactSteps),     "Fakta – trin for trin",      FactSteps.Sample),
            new("fact-two-images", typeof(FactTwoImages), "Fakta – to billeder",        FactTwoImages.Sample),
            new("fact-list",       typeof(FactList),      "Fakta – billede og punkter", FactList.Sample),
            new("fact-video",      typeof(FactVideo),     "Fakta – video med tekst",    FactVideo.Sample),
            new("fact-video-only", typeof(FactVideoOnly), "Fakta – kun video",          FactVideoOnly.Sample),

            //Quiz'

            //Games
        ];



        public static TemplateEntry? Find(string? templateType) =>
            All.FirstOrDefault(e => e.Type == templateType);
    }
}