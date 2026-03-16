using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Features.Members.UpdateMemberRole;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, BadRequest<Error>, ForbidHttpResult, UnauthorizedHttpResult>> UpdateMemberRole(
        [FromRoute] Guid id,
        [FromRoute] Guid userId,
        UpdateMemberRoleRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IAuthorizationService authorizationService,
        ICommandHandler<UpdateMemberRoleCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var accessContext = await db.GetAccessContextAsync(id, ct);

        if (accessContext is null || accessContext.OwnerId != userIdResult.Value)
        {
            return TypedResults.Forbid();
        }

        var result = await handler.HandleAsync(
            new UpdateMemberRoleCommand(id, userId, request.Role, request.CustomRoleName), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.NoContent();
    }
}