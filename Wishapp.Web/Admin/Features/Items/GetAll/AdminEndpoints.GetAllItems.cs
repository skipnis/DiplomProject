using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Items.GetAll;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<PagedResponse<CatalogItemDto>>> GetAllItems(
        [AsParameters] CatalogItemsRequest request,
        IQueryHandler<GetAllCatalogItemsQuery, PagedResponse<CatalogItemDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new GetAllCatalogItemsQuery(request.ToFilter(), request.ToPaged()), ct);
        return TypedResults.Ok(result.Value);
    }
}
