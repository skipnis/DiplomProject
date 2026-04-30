using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Features.UnvoteCatalogItemBadge;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Gamification;

public static partial class GamificationEndpoints
{
    private static async Task<Results<Ok, UnauthorizedHttpResult, NotFound>> UnvoteCatalogItemBadge(
        [FromRoute] Guid id,
        [FromRoute] int badgeType,
        ClaimsPrincipal user,
        ICommandHandler<UnvoteCatalogItemBadgeCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new UnvoteCatalogItemBadgeCommand(userIdResult.Value, id, badgeType), ct);

        return result.IsSuccess ? TypedResults.Ok() : TypedResults.NotFound();
    }
}
