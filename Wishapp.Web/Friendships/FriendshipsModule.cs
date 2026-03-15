namespace Wishapp.Web.Friendships;

public static class FriendshipsModule
{
    public static IServiceCollection AddFriendshipsModule(this IServiceCollection services)
    {
        services.AddScoped<IFriendshipsApi, FriendshipsApi>();
        
        return services;
    }
}
