using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Friendships.Features.AcceptFriend;
using Wishapp.Web.Friendships.Features.DeclineFriend;
using Wishapp.Web.Friendships.Features.GetFriends;
using Wishapp.Web.Friendships.Features.GetFriendshipRequests;
using Wishapp.Web.Friendships.Features.MakeFriend;
using Wishapp.Web.Friendships.Features.RemoveFriend;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Friendships;

public static class FriendshipsEndpoints
{
    public static IEndpointRouteBuilder MapFriendshipsEndpoints(this IEndpointRouteBuilder app)
    {
        var friendships = app.MapGroup("/friendships")
            .RequireAuthorization();

        friendships.MapPost("/{userId:guid}", MakeFriend)
            .Produces(401);;;

        friendships.MapPut("/{userId:guid}/accept", AcceptFriendRequest)
            .Produces(401);
        
        friendships.MapPut("/{userId:guid}/decline", DeclineFriend)
            .Produces(401);
        
        friendships.MapDelete("/{userId:guid}", RemoveFriend)
            .Produces(401);

        friendships.MapGet("/requests", GetFriendshipRequests)
            .Produces(401);
        
        friendships.MapGet("/", GetFriends)
            .Produces(401);
        
        return app;
    }

    private static async Task<Results<NoContent, NotFound<Error>, Conflict<Error>, UnauthorizedHttpResult, BadRequest<Error>>> MakeFriend(
        Guid userId,
        ClaimsPrincipal user,
        ICommandHandler<MakeFriendCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new MakeFriendCommand(userIdResult.Value, userId), ct);

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(result.Error),
            ErrorType.Conflict => TypedResults.Conflict(result.Error),
            _ => TypedResults.BadRequest(result.Error)
        };
    }

    private static async Task<Results<NoContent, NotFound<Error>, UnauthorizedHttpResult>> AcceptFriendRequest(
        Guid userId,
        ClaimsPrincipal user,
        ICommandHandler<AcceptFriendCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new AcceptFriendCommand(userIdResult.Value, userId), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
    
    private static async Task<Results<NoContent, NotFound<Error>, UnauthorizedHttpResult>> DeclineFriend(
        Guid userId,
        ClaimsPrincipal user,
        ICommandHandler<DeclineFriendCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new DeclineFriendCommand(userIdResult.Value, userId), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<Results<NoContent, NotFound<Error>, UnauthorizedHttpResult>> RemoveFriend(
        Guid userId,
        ClaimsPrincipal user,
        ICommandHandler<RemoveFriendCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new RemoveFriendCommand(userIdResult.Value, userId), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
    
    private static async Task<Results<Ok<IEnumerable<FriendshipRequest>>, UnauthorizedHttpResult>> GetFriendshipRequests(
        [FromQuery] FriendshipStatus status,
        ClaimsPrincipal user,
        IQueryHandler<GetFriendshipRequestsQuery, IEnumerable<FriendshipRequest>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(
            new GetFriendshipRequestsQuery(userIdResult.Value, status), ct);

        return TypedResults.Ok(result.Value);
    }
    
    private static async Task<Results<Ok<IEnumerable<FriendInfo>>, UnauthorizedHttpResult>> GetFriends(
        ClaimsPrincipal user,
        IQueryHandler<GetFriendsQuery, IEnumerable<FriendInfo>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            return TypedResults.Unauthorized();
        }

        var result = await handler.HandleAsync(new GetFriendsQuery(userIdResult.Value), ct);

        return TypedResults.Ok(result.Value);
    }
}