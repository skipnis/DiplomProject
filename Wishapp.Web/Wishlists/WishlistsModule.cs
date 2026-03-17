namespace Wishapp.Web.Wishlists;

public static class WishlistsModule
{
    public static IServiceCollection AddWishlistsModule(this IServiceCollection services)
    {
        services.AddScoped<IWishlistsApi, WishlistsApi>();
        
        return services;
    }
}