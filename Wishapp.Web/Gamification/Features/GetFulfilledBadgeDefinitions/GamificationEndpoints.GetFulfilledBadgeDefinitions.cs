using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Features.GetFulfilledBadgeDefinitions;

namespace Wishapp.Web.Gamification;

public static partial class GamificationEndpoints
{
    private static async Task<IResult> GetFulfilledBadgeDefinitions(
        IQueryHandler<GetFulfilledBadgeDefinitionsQuery, List<FulfilledBadgeDefinitionDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetFulfilledBadgeDefinitionsQuery(), ct);
        return Results.Ok(result.Value);
    }
}
