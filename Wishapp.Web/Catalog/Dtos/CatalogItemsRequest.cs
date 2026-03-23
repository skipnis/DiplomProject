using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Catalog.Dtos;

public record CatalogItemsRequest(
    Guid? CategoryId = null,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int Page = 1,
    int PageSize = 20)
{
    public CatalogItemsFilter ToFilter() => new(CategoryId, Search, MinPrice, MaxPrice);
    public PagedRequest ToPaged() => new(Page, PageSize);
}
