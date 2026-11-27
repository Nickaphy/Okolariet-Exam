using Web.Templates.Facts;
using Web.Templates.Menus.MenuTemplates;
using Web.Templates.Quiz;

namespace Web.Templates
{
    // One template the app can draw.
    //   Type      = the template type the server uses (TemplateCatalog in Infrastructure). Must match exactly.
    //   Component = the Blazor component that draws it.
    //   Sample    = example content for the template gallery and the template picker.
    //
    // There is no name here on purpose: the name staff see ("Fakta – tre tal") is the server's DisplayName,
    // so it is written in exactly one place (TemplateCatalog). The gallery shows it from there.
    public sealed record TemplateEntry(string Type, Type Component, Func<PageContent> Sample);

    // Which Blazor component draws which template. One line per template.
    //
    // Adding a template = 1) a Razor component in Templates/, 2) one line here, 3) one entry in TemplateCatalog
    // on the server with the same type and slot keys. The editor, the kiosk and the database need no changes.
    // The template gallery (Admin → Skabeloner) checks that this list and the server's catalog match.
    //
    // Written out by hand instead of found automatically (reflection), so WebAssembly's trimming
    // can never remove a template by accident, and so this list is easy to read.
    public static class TemplateRegistry
    {
        public static readonly IReadOnlyList<TemplateEntry> All =
        [
            // Main menus
            new("menu-buttons",       typeof(MenuButtons),      MenuButtons.Sample),
            new("menu-split-4",       typeof(MenuSplitFour),    MenuSplitFour.Sample),
            new("menu-three",         typeof(MenuThree),        MenuThree.Sample),
            new("menu-six",           typeof(MenuSix),          MenuSix.Sample),
            new("menu-eight",         typeof(MenuEight),        MenuEight.Sample),

            // Facts
            new("fact-image-text",    typeof(FactImageText),    FactImageText.Sample),
            new("fact-hero",          typeof(FactHero),         FactHero.Sample),
            new("fact-stats",         typeof(FactStats),        FactStats.Sample),
            new("fact-steps",         typeof(FactSteps),        FactSteps.Sample),
            new("fact-two-images",    typeof(FactTwoImages),    FactTwoImages.Sample),
            new("fact-list",          typeof(FactList),         FactList.Sample),
            new("fact-video",         typeof(FactVideo),        FactVideo.Sample),
            new("fact-video-only",    typeof(FactVideoOnly),    FactVideoOnly.Sample),

            // Quizzes (they share the game in Templates/Quiz/QuizRunner)
            new("quiz-classic",       typeof(QuizClassic),      QuizClassic.Sample),
            new("quiz-factbox",       typeof(QuizFactBox),      QuizFactBox.Sample),
            new("quiz-image-answers", typeof(QuizImageAnswers), QuizImageAnswers.Sample),
            new("quiz-video",         typeof(QuizVideo),        QuizVideo.Sample),
            new("quiz-true-false",    typeof(QuizTrueFalse),    QuizTrueFalse.Sample),
        ];

        public static TemplateEntry? Find(string? templateType) =>
            All.FirstOrDefault(e => e.Type == templateType);
    }
}
