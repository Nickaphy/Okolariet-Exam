namespace Web.Helpers;

// The address of an uploaded file (image, video, sound). Files are served by the API itself
// (GET /assets/{id}), so a plain <img src="..."> or <video src="..."> works, also in the kiosk.
// Registered once in Program.cs with the API address from appsettings.json.
public sealed class AssetUrls(Uri apiBaseUrl)
{
    private readonly string root = apiBaseUrl.ToString().TrimEnd('/');

    public string For(Guid assetId) => $"{root}/assets/{assetId}";
}
