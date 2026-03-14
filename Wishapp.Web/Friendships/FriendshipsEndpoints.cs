namespace Wishapp.Web.Friendships;

public static class FriendshipsEndpoints
{
    public static IEndpointRouteBuilder MapFriendshipsEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var friendshipsEndpoints = routeBuilder.MapGroup("/friendships");
        return routeBuilder;
    }
}
