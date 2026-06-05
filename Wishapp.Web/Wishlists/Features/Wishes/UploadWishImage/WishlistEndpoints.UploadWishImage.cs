using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Authorization.Requirements;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Entities;
using Wishapp.Web.Wishlists.Features.Wishes.UploadWishImage;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<Ok<UploadWishImageResponse>, BadRequest<Error>, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> UploadWishImage(
        [FromRoute] Guid id,
        [FromRoute] Guid wishId,
        [FromForm] UploadWishImageRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IAuthorizationService authorizationService,
        ICommandHandler<UploadWishImageCommand, UploadWishImageResponse> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var accessContext = await db.GetAccessContextAsync(id, ct);

        if (accessContext is null)
        {
            return TypedResults.Forbid();
        }

        var authorized = (await authorizationService
                .AuthorizeAsync(user, accessContext, new WishlistMemberRequirement(WishlistMemberRole.Editor)))
            .Succeeded;

        if (!authorized)
        {
            return TypedResults.Forbid();
        }

        var isWishlistOwner = accessContext.OwnerId == userIdResult.Value;

        if (!isWishlistOwner)
        {
            var wishCreatorId = await db.Wishes
                .AsNoTracking()
                .Where(w => w.Id == wishId && w.WishlistId == id)
                .Select(w => w.CreatedByUserId)
                .FirstOrDefaultAsync(ct);

            if (wishCreatorId != userIdResult.Value)
                return TypedResults.Forbid();
        }

        var result = await handler.HandleAsync(
            new UploadWishImageCommand(id, wishId, request.File, request.ExternalImageUrl), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.Ok(result.Value);
    }
}
