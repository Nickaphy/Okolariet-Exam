using Web.Services.Editor;

namespace Web.Templates
{
    // Index is 1-based.
    public sealed record PathPosition(int Index, int Total);

    // "Side 2 / 5": where a page sits in its chain of "next" links, on the published pages.
    // It follows whatever order staff chose. A page on its own (no next, nothing pointing at it) has no position.
    public static class PathProgress
    {
        public static PathPosition? Of(PageContent current, IReadOnlyDictionary<Guid, PageContent> pages)
        {
            // Who points at whom, so we can walk backwards to the start of the path.
            var previous = new Dictionary<Guid, Guid>();
            foreach (var page in pages.Values)
                if (NextOf(page) is Guid next && pages.ContainsKey(next))
                    previous.TryAdd(next, page.Id);

            var seen = new HashSet<Guid> { current.Id };   // also stops a loop

            var before = 0;
            for (var id = current.Id; previous.TryGetValue(id, out var earlier) && seen.Add(earlier); id = earlier)
                before++;

            var after = 0;
            for (var page = current;
                 NextOf(page) is Guid next && pages.TryGetValue(next, out var following) && seen.Add(next);
                 page = following)
                after++;

            return before + after == 0 ? null : new PathPosition(before + 1, before + after + 1);
        }

        private static Guid? NextOf(PageContent page) =>
            page.Slots.GetValueOrDefault(PathConvention.NextSlotKey)?.TargetId;
    }
}