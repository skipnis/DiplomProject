using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Features.VoteCatalogItemBadge;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Gamification;

public static partial class GamificationEndpoints
{
    private static async Task<Results<Ok, UnauthorizedHttpResult, NotFound>> VoteCatalogItemBadge(
        [FromRoute] Guid id,
        [FromRoute] int badgeType,
        ClaimsPrincipal user,
        ICommandHandler<VoteCatalogItemBadgeCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new VoteCatalogItemBadgeCommand(userIdResult.Value, id, badgeType), ct);

        return result.IsSuccess ? TypedResults.Ok() : TypedResults.NotFound();
    }
}
