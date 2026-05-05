namespace Wishapp.Web.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        services.AddScoped<ICatalogApi, CatalogApi>();

        return services;
    }
}
