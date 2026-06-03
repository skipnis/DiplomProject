using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.GetStats;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<AdminStatsResponse>> GetStats(
        IQueryHandler<GetAdminStatsQuery, AdminStatsResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetAdminStatsQuery(), ct);
        return TypedResults.Ok(result.Value);
    }
}
