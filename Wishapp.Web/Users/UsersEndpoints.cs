using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Users.Features.GetMyProfile;
using Wishapp.Web.Users.Features.GetUserProfile;
using Wishapp.Web.Users.Features.GoogleSignIn;
using Wishapp.Web.Users.Features.SearchUsers;
using Wishapp.Web.Users.Features.UpdateProfile;

namespace Wishapp.Web.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");
        
        auth.MapPost("/google", GoogleSignIn);

        var usersEndpoints = app.MapGroup("/users")
            .RequireAuthorization();
        
        usersEndpoints.MapGet("/me", GetMyProfile);
        
        usersEndpoints.MapPut("/me", UpdateProfile);
        
        usersEndpoints.MapGet("/{id:guid}", GetUserProfile);
        
        usersEndpoints.MapGet("/search", SearchUsers);
        
        return app;
    }
    
    private static async Task<Results<Ok<GoogleSignInResponse>, UnauthorizedHttpResult>> GoogleSignIn(
        GoogleSignInCommand command,
        ICommandHandler<GoogleSignInCommand, GoogleSignInResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Unauthorized();
    }

    private static async Task<Results<Ok<GetMyProfileResponse>, UnauthorizedHttpResult>> GetMyProfile(
        ClaimsPrincipal user,
        IQueryHandler<GetMyProfileQuery, GetMyProfileResponse> handler,
        CancellationToken ct)
    {
        var userId = user.TryGetUserId();

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }
        
        var result = await handler.HandleAsync(new GetMyProfileQuery(userId.Value), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Unauthorized();
    }
    
    private static async Task<Results<Ok<GetUserProfileResponse>, NotFound<Error>>> GetUserProfile(
        Guid id,
        ClaimsPrincipal user,
        IQueryHandler<GetUserProfileQuery, GetUserProfileResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetUserProfileQuery(id), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }
    
    private static async Task<Results<NoContent, NotFound<Error>, UnauthorizedHttpResult>> UpdateProfile(
        UpdateProfileRequest request,
        ClaimsPrincipal user,
        ICommandHandler<UpdateProfileCommand> handler,
        CancellationToken ct)
    {
        var userId = user.TryGetUserId();

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new UpdateProfileCommand
            (userId.Value, 
                request.Username,
                request.Bio, request.BirthDate), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<Ok<UsersSearchResponse>> SearchUsers(
        [FromQuery] string username,
        IQueryHandler<SearchUsersQuery, UsersSearchResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new SearchUsersQuery(username), ct);
        return TypedResults.Ok(result.Value);
    }
}
