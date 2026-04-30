using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Dtos;
using Wishapp.Web.Gamification.Features.GetMyGiftProfile;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Gamification;

public static partial class GamificationEndpoints
{
    private static async Task<Results<Ok<GiftProfileDto>, UnauthorizedHttpResult>> GetMyGiftProfile(
        ClaimsPrincipal user,
        IQueryHandler<GetMyGiftProfileQuery, GiftProfileDto> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new GetMyGiftProfileQuery(userIdResult.Value), ct);
        return TypedResults.Ok(result.Value);
    }
}
