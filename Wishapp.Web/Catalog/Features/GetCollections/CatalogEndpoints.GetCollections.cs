using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.GetCollections;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Ok<List<CatalogCollectionSummaryDto>>> GetCollections(
        IQueryHandler<GetCollectionsQuery, List<CatalogCollectionSummaryDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetCollectionsQuery(), ct);
        return TypedResults.Ok(result.Value);
    }
}
