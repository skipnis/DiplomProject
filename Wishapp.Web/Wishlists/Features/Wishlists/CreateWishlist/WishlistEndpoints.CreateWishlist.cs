using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Features.Wishlists.CreateWishlist;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    private static async Task<Results<Created<CreateWishlistResponse>, UnauthorizedHttpResult, NotFound<Error>, BadRequest<Error>>> CreateWishlist(
        CreateWishlistRequest request,
        ClaimsPrincipal user,
        ICommandHandler<CreateWishlistCommand, CreateWishlistResponse> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new CreateWishlistCommand(
                userIdResult.Value,
                request.Name,
                request.Description,
                request.Emoji,
                request.Visibility,
                request.Members), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.Created($"/wishlists/{result.Value.Id}", result.Value);
    }
}