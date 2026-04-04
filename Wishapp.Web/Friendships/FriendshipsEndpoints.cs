using Wishapp.Web.Friendships.Features.GetFriendshipRequests;
using Wishapp.Web.Infrastructure.Validation;

namespace Wishapp.Web.Friendships;

public static partial class FriendshipsEndpoints
{
    public static IEndpointRouteBuilder MapFriendshipsEndpoints(this IEndpointRouteBuilder app)
    {
        var friendships = app.MapGroup("/friendships")
            .RequireAuthorization();

        friendships.MapPost("/{userId:guid}", MakeFriend).Produces(401);
        friendships.MapPut("/{userId:guid}/accept", AcceptFriendRequest).Produces(401);
        friendships.MapPut("/{userId:guid}/decline", DeclineFriend).Produces(401);
        friendships.MapDelete("/{userId:guid}", RemoveFriend).Produces(401);
        friendships.MapGet("/requests", GetFriendshipRequests).Produces(401)
            .AddEndpointFilter<ValidationFilter<GetFriendshipRequestsRequest>>();
        friendships.MapGet("/", GetFriends).Produces(401);

        return app;
    }
}
