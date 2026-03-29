using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Authorization.Requirements;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Entities;
using Wishapp.Web.Wishlists.Features.Wishes.AddWishFromCatalog;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<Created<Guid>, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> AddWishFromCatalog(
        [FromRoute] Guid id,
        AddWishFromCatalogRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IAuthorizationService authorizationService,
        ICommandHandler<AddWishFromCatalogCommand, Guid> handler,
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

        var result = await handler.HandleAsync(
            new AddWishFromCatalogCommand(id, request.CatalogItemId, userIdResult.Value), ct);

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound(result.Error);
        }

        return TypedResults.Created($"/wishlists/{id}/wishes/{result.Value}", result.Value);
    }
}

public record AddWishFromCatalogRequest(Guid CatalogItemId);
