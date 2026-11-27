namespace Web.Components
{
    // A message at the top of a page ("Udstillingen er slettet.", an error ...). Shown by NoticeBar.
    // Kind is "success", "error" or "info"; it picks the colour and the icon.
    // Every page keeps one field:  private Notice? notice;  and sets it with Notice.Success(...) / Notice.Error(...).
    public sealed record Notice(string Kind, string Text)
    {
        public static Notice Success(string text) => new("success", text);
        public static Notice Error(string text) => new("error", text);
        public static Notice Info(string text) => new("info", text);
    }

    // Where a page is with fetching its data. Used to choose between "Henter…", an error with "Prøv igen", or the content.
    public enum LoadState { Loading, Loaded, Failed }
}
