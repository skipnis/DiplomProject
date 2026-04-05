using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Catalog.Features.RateCatalogItem;
using Wishapp.Web.Catalog.Features.UnrateCatalogItem;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Results<Ok, UnauthorizedHttpResult, NotFound>> RateCatalogItem(
        [FromRoute] Guid id,
        RateCatalogItemRequest request,
        ClaimsPrincipal user,
        ICommandHandler<RateCatalogItemCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(new RateCatalogItemCommand(userIdResult.Value, id, request.Value), ct);

        return result.IsSuccess ? TypedResults.Ok() : TypedResults.NotFound();
    }

    private static async Task<Results<Ok, UnauthorizedHttpResult>> UnrateCatalogItem(
        [FromRoute] Guid id,
        ClaimsPrincipal user,
        ICommandHandler<UnrateCatalogItemCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        await handler.HandleAsync(new UnrateCatalogItemCommand(userIdResult.Value, id), ct);

        return TypedResults.Ok();
    }
}
