using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Users.Features.SendOtp;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    private static async Task<Results<Ok, BadRequest<string>>> SendOtp(
        SendOtpCommand command,
        ICommandHandler<SendOtpCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error.Description);

        return TypedResults.Ok();
    }
}
