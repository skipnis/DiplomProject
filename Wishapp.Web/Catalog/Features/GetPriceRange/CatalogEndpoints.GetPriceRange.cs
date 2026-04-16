using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Catalog.Features.GetPriceRange;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Ok<PriceRangeResult>> GetPriceRange(
        IQueryHandler<GetPriceRangeQuery, PriceRangeResult> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetPriceRangeQuery(), ct);
        return TypedResults.Ok(result.Value);
    }
}
