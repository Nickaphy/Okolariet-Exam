namespace Application.Helpers
{ //A PagedList is a list of items that is paged. "Paging" in our case is done to help prevent the API from calling 100000 of words at once, instead it helps batch it.
  //It contains the items, the page number, the page size, and the total count of items. Sort of like h
    public record PagedList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
}
