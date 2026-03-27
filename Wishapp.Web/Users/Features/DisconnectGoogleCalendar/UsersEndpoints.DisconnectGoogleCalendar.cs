using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.DisconnectGoogleCalendar;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<NoContent, UnauthorizedHttpResult>> DisconnectGoogleCalendar(
        ClaimsPrincipal user,
        ICommandHandler<DisconnectGoogleCalendarCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure) return TypedResults.Unauthorized();

        await handler.HandleAsync(new DisconnectGoogleCalendarCommand(userIdResult.Value), ct);

        return TypedResults.NoContent();
    }
}
