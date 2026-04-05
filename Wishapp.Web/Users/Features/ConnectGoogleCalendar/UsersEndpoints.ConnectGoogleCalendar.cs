using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.ConnectGoogleCalendar;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<NoContent, BadRequest<Error>, UnauthorizedHttpResult>> ConnectGoogleCalendar(
        [FromBody] ConnectGoogleCalendarRequest request,
        ClaimsPrincipal user,
        ICommandHandler<ConnectGoogleCalendarCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new ConnectGoogleCalendarCommand(userIdResult.Value, request.Code), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(result.Error);
    }
}
