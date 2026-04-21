using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.GetOccasions;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Ok<List<OccasionDto>>> GetOccasions(
        IQueryHandler<GetOccasionsQuery, List<OccasionDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetOccasionsQuery(), ct);
        return TypedResults.Ok(result.Value);
    }
}
