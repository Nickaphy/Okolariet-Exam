using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Web.Api;
using Web.Api.Contracts;
using Web.Services.Editor;
using Web.Templates;
using Web.Theming;

namespace Web.Pages.Kiosk
{
    public partial class KioskStart
    {
        [Parameter] public Guid ProjectId { get; set; }

        private PublishedVersion? version;

        // Every page of the published version, by id, so a button's target can be looked up directly.
        private Dictionary<Guid, PageContent> pages = new();

        // The page on screen now.
        private PageContent? current;

        // The pages visited before this one, newest on top. "Tilbage" takes the top one off again.
        private readonly Stack<Guid> history = new();

        private string? errorMessage;
        private bool isFullscreen;

        // The page after this one in its path (the hidden "next" slot), if there is one.
        private Guid? NextId =>
            current?.Slots.GetValueOrDefault(PathConvention.NextSlotKey)?.TargetId is Guid id && pages.ContainsKey(id)
                ? id
                : null;

        // "Side 2 / 5" in the bar, worked out from the "next" links (see PathProgress).
        private PathPosition? Progress => current is null ? null : PathProgress.Of(current, pages);

        // The page's colours on the whole window too, so the bars around the 16:9 screen and the nav buttons match it.
        private string KioskStyle => current is null ? string.Empty : Theme.Style(current.Appearance, version?.Appearance);

        private PageContent? MainMenu => version is null ? null : pages.GetValueOrDefault(version.MainModdedTemplateId);

        protected override async Task OnParametersSetAsync()
        {
            var (latest, error) = await ApiCall.Get(() => VersionGateway.GetLatestVersion(ProjectId),
                (HttpStatusCode.NotFound, "Udstillingen er ikke udgivet endnu."));
            if (error is not null)
            {
                errorMessage = error;
                return;
            }

            version = latest!;
            pages = (version.ModdedTemplates ?? []).Select(p => PageContent.From(p)).ToDictionary(p => p.Id);
            history.Clear();

            current = MainMenu ?? pages.Values.FirstOrDefault();
            if (current is null) errorMessage = "Udstillingen har ingen sider.";
        }

        // A button (or "Videre") was tapped: remember where we were, and show the target page.
        private void GoTo(Guid id)
        {
            if (current is null || !pages.TryGetValue(id, out var target)) return;
            history.Push(current.Id);
            current = target;
        }

        private void Back()
        {
            if (history.Count > 0 && pages.TryGetValue(history.Pop(), out var previous))
                current = previous;
        }

        private void Home()
        {
            history.Clear();
            current = MainMenu ?? current;
        }

        // Once the kiosk is on screen: switch off right-click, dragging, selecting and the developer-tools keys (wwwroot/js).
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender) await JS.InvokeVoidAsync("oekolariet.lockKiosk");
        }

        private async Task EnterFullscreenAsync()
        {
            // Must run from a tap: browsers only allow full screen after the user did something.
            // If the browser refuses, the overlay is hidden anyway, so the kiosk stays usable.
            await JS.InvokeAsync<bool>("oekolariet.enterFullscreen");
            isFullscreen = true;
        }
    }
}
