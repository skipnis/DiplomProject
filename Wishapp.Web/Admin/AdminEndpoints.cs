using Wishapp.Web.Admin.Features.Categories.Create;
using Wishapp.Web.Admin.Features.Categories.Update;
using Wishapp.Web.Admin.Features.Collections.Create;
using Wishapp.Web.Admin.Features.Collections.Update;
using Wishapp.Web.Admin.Features.Items.Create;
using Wishapp.Web.Admin.Features.Items.Update;
using Wishapp.Web.Admin.Features.Items.UploadImage;
using Wishapp.Web.Infrastructure.Validation;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/admin");

        admin.MapPost("/auth/login", Login);

        var secured = admin.MapGroup("/catalog").RequireAuthorization("Admin");

        secured.MapPost("/categories", CreateCategory)
            .AddEndpointFilter<ValidationFilter<CreateCategoryCommand>>();
        secured.MapPut("/categories/{id:guid}", UpdateCategory)
            .AddEndpointFilter<ValidationFilter<UpdateCategoryRequest>>();
        secured.MapDelete("/categories/{id:guid}", DeleteCategory);

        secured.MapGet("/items", GetAllItems);
        secured.MapPost("/items", CreateItem)
            .AddEndpointFilter<ValidationFilter<CreateCatalogItemCommand>>();
        secured.MapPut("/items/{id:guid}", UpdateItem)
            .AddEndpointFilter<ValidationFilter<UpdateCatalogItemRequest>>();
        secured.MapDelete("/items/{id:guid}", DeleteItem);
        secured.MapPost("/items/{id:guid}/image", UploadCatalogItemImage).DisableAntiforgery()
            .AddEndpointFilter<ValidationFilter<UploadCatalogItemImageRequest>>();
        secured.MapPatch("/items/{id:guid}/published", SetCatalogItemPublished);

        secured.MapGet("/collections", GetAllCollections);
        secured.MapPost("/collections", CreateCollection)
            .AddEndpointFilter<ValidationFilter<CreateCollectionCommand>>();
        secured.MapPut("/collections/{id:guid}", UpdateCollection)
            .AddEndpointFilter<ValidationFilter<UpdateCollectionRequest>>();
        secured.MapDelete("/collections/{id:guid}", DeleteCollection);
        secured.MapPost("/collections/{id:guid}/image", UploadCollectionCover).DisableAntiforgery();
        secured.MapGet("/collections/{id:guid}/items", GetCollectionItems);
        secured.MapPost("/collections/{id:guid}/items/{itemId:guid}", AddItemToCollection);
        secured.MapDelete("/collections/{id:guid}/items/{itemId:guid}", RemoveItemFromCollection);

        return app;
    }
}
