using Web.Services.Editor;

namespace Web.Templates
{
    // "Page 2 of 5". Index starts at 1.
    public sealed record PathPosition(int Index, int Total);

    // Works out where the kiosk's current page sits in its path, for "Side 2 / 5" and the dots.
    //
    // The published pages don't store an order; each page only knows the page AFTER it (the "next" slot,
    // see PathConvention). So we count:
    //   - backwards: which page points at this one, which page points at that one, ... → pages before;
    //   - forwards:  this page's next, that page's next, ...                            → pages after.
    // It follows whatever order staff chose in the editor. A page alone (nothing before or after) has no position.
    public static class PathProgress
    {
        public static PathPosition? Of(PageContent current, IReadOnlyDictionary<Guid, PageContent> pages)
        {
            // Turn the "next" links around: for every page, which page comes BEFORE it.
            var previousOf = new Dictionary<Guid, Guid>();
            foreach (var page in pages.Values)
            {
                if (NextOf(page) is Guid next && pages.ContainsKey(next))
                    previousOf.TryAdd(next, page.Id);
            }

            // Pages already counted. Stops us from counting forever if the links ever form a loop (A → B → A).
            var seen = new HashSet<Guid> { current.Id };

            var before = 0;
            var id = current.Id;
            while (previousOf.TryGetValue(id, out var earlier) && seen.Add(earlier))
            {
                before++;
                id = earlier;
            }

            var after = 0;
            var at = current;
            while (NextOf(at) is Guid next && pages.TryGetValue(next, out var following) && seen.Add(next))
            {
                after++;
                at = following;
            }

            if (before + after == 0) return null;
            return new PathPosition(before + 1, before + after + 1);
        }

        private static Guid? NextOf(PageContent page) =>
            page.Slots.GetValueOrDefault(PathConvention.NextSlotKey)?.TargetId;
    }
}
