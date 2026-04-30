using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Dtos;
using Wishapp.Web.Gamification.Features.GetGiftBadges;

namespace Wishapp.Web.Gamification;

public static partial class GamificationEndpoints
{
    private static async Task<Results<Ok<List<FulfilledWishBadgeDto>>, NotFound>> GetGiftBadges(
        [FromRoute] Guid id,
        [FromRoute] Guid wishId,
        IQueryHandler<GetGiftBadgesQuery, List<FulfilledWishBadgeDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetGiftBadgesQuery(id, wishId), ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.NotFound();
    }
}
