namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/admin");

        admin.MapPost("/auth/login", Login);

        var secured = admin.MapGroup("/catalog").RequireAuthorization("Admin");

        secured.MapPost("/categories", CreateCategory);
        secured.MapPut("/categories/{id:guid}", UpdateCategory);
        secured.MapDelete("/categories/{id:guid}", DeleteCategory);

        secured.MapGet("/items", GetAllItems);
        secured.MapPost("/items", CreateItem);
        secured.MapPut("/items/{id:guid}", UpdateItem);
        secured.MapDelete("/items/{id:guid}", DeleteItem);
        secured.MapPost("/items/{id:guid}/image", UploadCatalogItemImage).DisableAntiforgery();
        secured.MapPatch("/items/{id:guid}/published", SetCatalogItemPublished);

        secured.MapGet("/collections", GetAllCollections);
        secured.MapPost("/collections", CreateCollection);
        secured.MapPut("/collections/{id:guid}", UpdateCollection);
        secured.MapDelete("/collections/{id:guid}", DeleteCollection);
        secured.MapPost("/collections/{id:guid}/image", UploadCollectionCover).DisableAntiforgery();
        secured.MapGet("/collections/{id:guid}/items", GetCollectionItems);
        secured.MapPost("/collections/{id:guid}/items/{itemId:guid}", AddItemToCollection);
        secured.MapDelete("/collections/{id:guid}/items/{itemId:guid}", RemoveItemFromCollection);

        return app;
    }
}
