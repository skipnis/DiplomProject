using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Catalog.Dtos;

public record CatalogItemsRequest(
    Guid? CategoryId = null,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    [property: FromQuery(Name = "occasionIds")] List<Guid>? OccasionIds = null,
    int Page = 1,
    int PageSize = 20)
{
    public CatalogItemsFilter ToFilter() => new(CategoryId, Search, MinPrice, MaxPrice, OccasionIds);
    public PagedRequest ToPaged() => new(Page, PageSize);
}
