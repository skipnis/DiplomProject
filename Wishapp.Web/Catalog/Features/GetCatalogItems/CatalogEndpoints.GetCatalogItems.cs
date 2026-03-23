using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.GetCatalogItems;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Ok<PagedResponse<CatalogItemDto>>> GetCatalogItems(
        [AsParameters] CatalogItemsRequest request,
        IQueryHandler<GetCatalogItemsQuery, PagedResponse<CatalogItemDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new GetCatalogItemsQuery(request.ToFilter(), request.ToPaged()), ct);
        return TypedResults.Ok(result.Value);
    }
}
