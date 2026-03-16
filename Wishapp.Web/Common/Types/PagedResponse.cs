namespace Wishapp.Web.Common.Types;

public record PagedResponse<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
}