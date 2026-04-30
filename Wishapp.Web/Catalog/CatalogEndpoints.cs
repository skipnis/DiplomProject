using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Infrastructure.Validation;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var catalog = app.MapGroup("/catalog");

        catalog.MapGet("/categories", GetCategories);
        catalog.MapGet("/price-range", GetPriceRange);
        catalog.MapGet("/items", GetCatalogItems)
            .AddEndpointFilter<ValidationFilter<CatalogItemsRequest>>();
        catalog.MapGet("/items/{id:guid}", GetCatalogItem);
        catalog.MapGet("/occasions", GetOccasions);
        catalog.MapGet("/collections", GetCollections);
        catalog.MapGet("/collections/{id:guid}", GetCollection);

        return app;
    }
}
