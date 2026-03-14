using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.GoogleSignIn;

namespace Wishapp.Web.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");
        
        auth.MapPost("/google", GoogleSignIn);
        
        var usersEndpoints = app.MapGroup("/users");
        
        return app;
    }
    
    private static async Task<Results<Ok<GoogleSignInResponse>, UnauthorizedHttpResult>> GoogleSignIn(
        GoogleSignInCommand command,
        ICommandHandler<GoogleSignInCommand, GoogleSignInResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(command, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Unauthorized();
    }
}
