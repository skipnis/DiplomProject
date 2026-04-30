using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Dtos;
using Wishapp.Web.Gamification.Features.GetUserGiftProfile;

namespace Wishapp.Web.Gamification;

public static partial class GamificationEndpoints
{
    private static async Task<Results<Ok<GiftProfileDto>, NotFound<Error>>> GetUserGiftProfile(
        [FromRoute] Guid id,
        IQueryHandler<GetUserGiftProfileQuery, GiftProfileDto> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetUserGiftProfileQuery(id), ct);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }
}
