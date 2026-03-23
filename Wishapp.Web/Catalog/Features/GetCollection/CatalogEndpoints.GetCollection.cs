using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.GetCollection;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Results<Ok<CatalogCollectionDto>, NotFound<Error>>> GetCollection(
        [FromRoute] Guid id,
        IQueryHandler<GetCollectionQuery, CatalogCollectionDto> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetCollectionQuery(id), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }
}
