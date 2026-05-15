using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.AddBlacklistItem;
using Wishapp.Web.Users.Features.GetMyBlacklist;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Created<BlacklistItemResponse>, UnprocessableEntity<Error>, UnauthorizedHttpResult>> AddBlacklistItem(
        AddBlacklistItemRequest request,
        ClaimsPrincipal user,
        ICommandHandler<AddBlacklistItemCommand, BlacklistItemResponse> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new AddBlacklistItemCommand(userIdResult.Value, request.Title), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/users/me/blacklist/{result.Value.Id}", result.Value)
            : TypedResults.UnprocessableEntity(result.Error);
    }
}
