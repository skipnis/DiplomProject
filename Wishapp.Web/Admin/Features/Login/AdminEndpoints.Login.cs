using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Login;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<Ok<AdminLoginResponse>, UnauthorizedHttpResult>> Login(
        AdminLoginCommand command,
        ICommandHandler<AdminLoginCommand, AdminLoginResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Unauthorized();
    }
}
