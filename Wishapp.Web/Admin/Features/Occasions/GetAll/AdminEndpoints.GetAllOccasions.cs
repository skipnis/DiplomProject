using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.GetOccasions;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<OccasionDto>>> GetAllOccasions(
        IQueryHandler<GetOccasionsQuery, List<OccasionDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetOccasionsQuery(), ct);
        return TypedResults.Ok(result.Value);
    }
}
