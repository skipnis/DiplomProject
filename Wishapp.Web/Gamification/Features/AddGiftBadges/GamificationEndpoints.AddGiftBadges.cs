using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Features.AddGiftBadges;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Gamification;

public static partial class GamificationEndpoints
{
    private static async Task<Results<Ok, UnauthorizedHttpResult, NotFound, Conflict<Error>, BadRequest<Error>>> AddGiftBadges(
        [FromRoute] Guid id,
        [FromRoute] Guid wishId,
        AddGiftBadgesRequest request,
        ClaimsPrincipal user,
        ICommandHandler<AddGiftBadgesCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new AddGiftBadgesCommand(userIdResult.Value, id, wishId, request.BadgeTypes), ct);

        if (result.IsSuccess)
            return TypedResults.Ok();

        return result.Error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(),
            ErrorType.Conflict => TypedResults.Conflict(result.Error),
            _ => TypedResults.BadRequest(result.Error)
        };
    }
}
