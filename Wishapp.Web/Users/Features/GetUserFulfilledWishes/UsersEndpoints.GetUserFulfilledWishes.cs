using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Users.Features.GetUserFulfilledWishes;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok<List<PublicFulfilledWishDto>>, NotFound<Error>>> GetUserFulfilledWishes(
        [FromRoute] Guid id,
        IQueryHandler<GetUserFulfilledWishesQuery, List<PublicFulfilledWishDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetUserFulfilledWishesQuery(id), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }
}
