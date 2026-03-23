namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var catalog = app.MapGroup("/catalog");

        catalog.MapGet("/categories", GetCategories);
        catalog.MapGet("/items", GetCatalogItems);
        catalog.MapGet("/items/{id:guid}", GetCatalogItem);
        catalog.MapGet("/collections", GetCollections);
        catalog.MapGet("/collections/{id:guid}", GetCollection);

        return app;
    }
}
