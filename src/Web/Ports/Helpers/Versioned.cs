namespace Web.Ports.Helpers
{
    // Something fetched together with its ETag: the server's "version stamp" for exactly this state of it.
    // Sending the ETag back as If-Match when saving makes the server refuse the save (412) if someone else
    // changed it in the meantime, so two staff members never overwrite each other without noticing.
    public sealed record Versioned<T>(T Value, string? ETag);
}
