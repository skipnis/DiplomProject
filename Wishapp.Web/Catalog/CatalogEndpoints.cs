using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.RateCatalogItem;
using Wishapp.Web.Infrastructure.Validation;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var catalog = app.MapGroup("/catalog");

        catalog.MapGet("/categories", GetCategories);
        catalog.MapGet("/items", GetCatalogItems)
            .AddEndpointFilter<ValidationFilter<CatalogItemsRequest>>();
        catalog.MapGet("/items/{id:guid}", GetCatalogItem);
        catalog.MapPost("/items/{id:guid}/rate", RateCatalogItem)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<RateCatalogItemRequest>>();
        catalog.MapDelete("/items/{id:guid}/rate", UnrateCatalogItem)
            .RequireAuthorization();
        catalog.MapGet("/collections", GetCollections);
        catalog.MapGet("/collections/{id:guid}", GetCollection);

        return app;
    }
}
