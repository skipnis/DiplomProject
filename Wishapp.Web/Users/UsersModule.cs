namespace Wishapp.Web.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUsersApi, UsersApi>();
        
        return services;
    }
}
