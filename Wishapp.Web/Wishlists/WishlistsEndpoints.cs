namespace Wishapp.Web.Wishlists;

public static class WishlistsEndpoints
{
    public static IEndpointRouteBuilder MapWishlistsEndpoints(this IEndpointRouteBuilder app)
    {
        var wishlistsEndpoints = app.MapGroup("/wishlists");
        return app;
    }
}
