using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Collections.GetItems;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<CatalogItemDto>>> GetCollectionItems(
        [FromRoute] Guid id,
        IQueryHandler<GetCollectionItemsQuery, List<CatalogItemDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetCollectionItemsQuery(id), ct);
        return TypedResults.Ok(result.Value);
    }
}
