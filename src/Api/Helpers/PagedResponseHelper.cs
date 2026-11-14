namespace Api.Helpers
{
    // Page unwrapper that helps to convert a PagedResult<T> into a PagedResponse<T> for API responses. This is useful for returning paginated data in a consistent format.
    public record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
}
