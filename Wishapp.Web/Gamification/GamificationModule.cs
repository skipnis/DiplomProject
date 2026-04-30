namespace Wishapp.Web.Gamification;

public static class GamificationModule
{
    public static IServiceCollection AddGamificationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GiftLevelOptions>(configuration.GetSection("GiftLevels"));
        services.AddScoped<IGamificationApi, GamificationApi>();

        return services;
    }
}
