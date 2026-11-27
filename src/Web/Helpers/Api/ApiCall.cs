using System.Net;
using Web.Exceptions;
using Web.Ports.Helpers;

namespace Web.Api;

// How pages and services call a gateway. Every call ends the same way: either a result,
// or a Danish error message (from ApiErrors). Pages never write try/catch themselves.
//
// The gateways (Ports/ + Adapters/) throw when something goes wrong:
//   GatewayException     = the API answered with an error status (404, 409 ...);
//   HttpRequestException = no connection at all.
// The methods below catch exactly those two and turn them into a message. Anything else is a real bug
// and is allowed to crash, so it gets noticed.
public static class ApiCall
{
    // A call that returns nothing (save, delete). null = it worked, otherwise the message.
    //     var error = await ApiCall.Run(() => Projects.DeleteProject(id), ignoreNotFound: true);
    // ignoreNotFound: a 404 counts as "worked" (someone else already deleted it, which is what the user wanted).
    public static async Task<string?> Run(Func<Task> call, bool ignoreNotFound = false,
        params (HttpStatusCode Status, string Message)[] overrides)
    {
        try
        {
            await call();
            return null;
        }
        catch (GatewayException e) when (ignoreNotFound && e.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception e) when (e is GatewayException or HttpRequestException)
        {
            return ApiErrors.For(e, overrides);
        }
    }

    // A call that returns something. Exactly one of the two is set.
    //     var (project, error) = await ApiCall.Get(() => Projects.CreateProject(input));
    public static async Task<(T? Value, string? Error)> Get<T>(Func<Task<T>> call,
        params (HttpStatusCode Status, string Message)[] overrides) where T : class
    {
        try
        {
            return (await call(), null);
        }
        catch (Exception e) when (e is GatewayException or HttpRequestException)
        {
            return (null, ApiErrors.For(e, overrides));
        }
    }

    // Every item of a paged list (the API gives at most 100 per page).
    //     var (projects, error) = await ApiCall.GetAll(page => Projects.ListProjects(page, 100));
    public static async Task<(List<T> Items, string? Error)> GetAll<T>(Func<int, Task<PagedResult<T>>> fetchPage)
    {
        var all = new List<T>();
        for (var page = 1; ; page++)
        {
            var (result, error) = await Get(() => fetchPage(page));
            if (error is not null) return (new List<T>(), error);

            all.AddRange(result!.Items);

            // Stop when everything is here, or on an empty page (so we can never loop forever).
            if (result.Items.Count == 0 || all.Count >= result.TotalCount) return (all, null);
        }
    }

    // Runs work that already returns "null = it worked, or a message", and only adds the missing piece:
    // a gateway exception in the middle of it becomes a message too. Used by the dialogs (Submit/Run),
    // ExhibitionEditor and AppearanceEditor.
    public static async Task<string?> Try(Func<Task<string?>> work)
    {
        try
        {
            return await work();
        }
        catch (Exception e) when (e is GatewayException or HttpRequestException)
        {
            return ApiErrors.For(e);
        }
    }
}
