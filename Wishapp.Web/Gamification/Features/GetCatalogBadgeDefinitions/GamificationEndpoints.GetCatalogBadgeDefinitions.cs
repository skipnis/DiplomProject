using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Features.GetCatalogBadgeDefinitions;

namespace Wishapp.Web.Gamification;

public static partial class GamificationEndpoints
{
    private static async Task<IResult> GetBadgeDefinitions(
        IQueryHandler<GetCatalogBadgeDefinitionsQuery, List<CatalogBadgeDefinitionDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetCatalogBadgeDefinitionsQuery(), ct);
        return Results.Ok(result.Value);
    }
}
