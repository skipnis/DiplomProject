using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.GetCatalogItem;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Results<Ok<CatalogItemDto>, NotFound<Error>>> GetCatalogItem(
        [FromRoute] Guid id,
        IQueryHandler<GetCatalogItemQuery, CatalogItemDto> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetCatalogItemQuery(id), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }
}
