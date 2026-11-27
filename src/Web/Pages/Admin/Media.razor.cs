using System.Net;
using Microsoft.AspNetCore.Components.Forms;
using Web.Api;
using Web.Api.Contracts;
using Web.Helpers;
using Web.Components;

namespace Web.Pages.Admin
{
    public partial class Media
    {
        private const int PageSize = 24;
        private const int MaxFilesAtOnce = 20;

        // Suggested limits from the API spec. The server has the last word (413 if a file is too big),
        // but checking here saves a long upload that is going to fail anyway.
        private const long ImageLimit = 10L * 1024 * 1024;
        private const long AudioLimit = 50L * 1024 * 1024;
        private const long VideoLimit = 500L * 1024 * 1024;

        private const string AcceptTypes =
            "image/png,image/jpeg,image/webp,image/gif,video/mp4,video/webm,video/x-matroska,.mkv,audio/mpeg,audio/wav,audio/mp4,.mp3,.wav,.m4a";

        private static readonly (string Label, AssetKind? Kind)[] Filters =
        [
            ("Alle", null),
            ("Billeder", AssetKind.Image),
            ("Video", AssetKind.Video),
            ("Lyd", AssetKind.Audio)
        ];

        private LoadState state = LoadState.Loading;
        private List<Asset> assets = [];
        private int total;
        private int page = 1;
        private AssetKind? kind;
        private string? loadError;
        private bool loadingMore;

        private bool uploading;
        private string? uploadStatus;
        private readonly List<string> uploadErrors = [];
        private int inputKey;

        private Notice? notice;
        private Asset? assetToDelete;

        private static string KindName(AssetKind assetKind) => assetKind switch
        {
            AssetKind.Image => "Billede",
            AssetKind.Video => "Video",
            _ => "Lyd"
        };

        protected override Task OnInitializedAsync() => LoadAsync();

        // ---- loading

        // The first page of files (of the chosen kind). The library can be large, so it is NOT fetched all at once:
        // "Vis flere" (LoadMoreAsync) fetches the next page.
        private async Task LoadAsync()
        {
            state = LoadState.Loading;
            page = 1;

            var (result, error) = await ApiCall.Get(() => AssetGateway.ListAssets(kind, 1, PageSize));
            if (error is not null)
            {
                loadError = error;
                state = LoadState.Failed;
                return;
            }

            assets = result!.Items.ToList();
            total = result.TotalCount;
            state = LoadState.Loaded;
        }

        private async Task LoadMoreAsync()
        {
            loadingMore = true;

            var (result, error) = await ApiCall.Get(() => AssetGateway.ListAssets(kind, page + 1, PageSize));
            if (error is not null)
            {
                notice = Notice.Error(error);
            }
            else
            {
                page++;
                assets.AddRange(result!.Items);
                total = result.TotalCount;
            }

            loadingMore = false;
        }

        private async Task SetKindAsync(AssetKind? value)
        {
            if (kind == value) return;

            kind = value;
            await LoadAsync();
        }

        // ---- upload

        private async Task OnFilesSelectedAsync(InputFileChangeEventArgs e)
        {
            uploadErrors.Clear();
            notice = null;

            if (e.FileCount > MaxFilesAtOnce)
            {
                notice = Notice.Error($"Vælg højst {MaxFilesAtOnce} filer ad gangen.");
                inputKey++;
                return;
            }

            var files = e.GetMultipleFiles(MaxFilesAtOnce);
            uploading = true;
            var uploaded = 0;

            try
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    uploadStatus = $"Uploader {i + 1} af {files.Count}: {file.Name}";
                    StateHasChanged();

                    var limit = LimitFor(file);
                    if (limit is null)
                    {
                        uploadErrors.Add($"{file.Name}: Filtypen understøttes ikke.");
                        continue;
                    }

                    if (file.Size > limit)
                    {
                        uploadErrors.Add($"{file.Name}: Filen er for stor (højst {Display.FileSize(limit.Value)}).");
                        continue;
                    }

                    // Without the limit argument Blazor only allows 500 KB.
                    await using var stream = file.OpenReadStream(limit.Value);
                    var contentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType;

                    var error = await ApiCall.Run(() => AssetGateway.UploadAsset(stream, file.Name, contentType), false,
                        (HttpStatusCode.BadRequest, "Filtypen understøttes ikke, eller filen er ødelagt."),
                        (HttpStatusCode.UnsupportedMediaType, "Filtypen understøttes ikke."));

                    if (error is null)
                    {
                        uploaded++;
                        continue;
                    }

                    uploadErrors.Add($"{file.Name}: {error}");
                    if (error == ApiErrors.Offline) break;   // no connection: the rest would fail the same way
                }
            }
            finally
            {
                uploading = false;
                uploadStatus = null;
                inputKey++; // a fresh <InputFile>, so the same file can be picked again
            }

            if (uploaded > 0)
            {
                notice = Notice.Success(uploaded == 1 ? "1 fil er uploadet." : $"{uploaded} filer er uploadet.");
                await LoadAsync();
            }
        }

        // The size limit depends on what kind of file it is. Null means "not something we accept".
        private static long? LimitFor(IBrowserFile file)
        {
            var type = file.ContentType ?? string.Empty;
            var name = file.Name.ToLowerInvariant();

            if (type.StartsWith("image/")) return ImageLimit;
            if (type.StartsWith("audio/") || name.EndsWith(".mp3") || name.EndsWith(".wav") || name.EndsWith(".m4a")) return AudioLimit;
            if (type.StartsWith("video/") || name.EndsWith(".mkv")) return VideoLimit;
            return null;
        }

        // ---- delete

        // Handed to ConfirmDialog as Run. ignoreNotFound: already gone is fine. 409: the file is still used on a page.
        private Task<string?> DeleteAsync() =>
            ApiCall.Run(() => AssetGateway.DeleteAsset(assetToDelete!.Id), ignoreNotFound: true,
                (HttpStatusCode.Conflict, "Filen bruges stadig på en side. Fjern den derfra først."));

        private async Task OnDeletedAsync()
        {
            var deleted = assetToDelete;
            assetToDelete = null;
            if (deleted is null) return;

            assets.Remove(deleted);
            total = Math.Max(0, total - 1);
            notice = Notice.Success($"«{deleted.OriginalName}» er slettet.");

            // Deleting the last file on a page can leave the list empty while more files exist.
            if (assets.Count == 0 && total > 0) await LoadAsync();
        }
    }
}