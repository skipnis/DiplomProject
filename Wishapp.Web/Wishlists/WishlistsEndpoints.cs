using Wishapp.Web.Infrastructure.Validation;
using Wishapp.Web.Wishlists.Features.Wishes.AddWish;
using Wishapp.Web.Wishlists.Features.Wishes.UpdateWish;
using Wishapp.Web.Wishlists.Features.Wishlists.CreateWishlist;
using Wishapp.Web.Wishlists.Features.Wishlists.UpdateWishlist;

namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    public static IEndpointRouteBuilder MapWishlistsEndpoints(this IEndpointRouteBuilder app)
    {
        var wishlists = app.MapGroup("/wishlists").RequireAuthorization();

        wishlists.MapPost("/", CreateWishlist)
            .AddEndpointFilter<ValidationFilter<CreateWishlistRequest>>();

        wishlists.MapGet("/{id:guid}", GetWishlist)
            .AllowAnonymous();

        wishlists.MapGet("/", GetMyWishlists);

        wishlists.MapGet("/users/{userId:guid}", GetUserWishlists)
            .AllowAnonymous();

        wishlists.MapPut("/{id:guid}", UpdateWishlist)
            .AddEndpointFilter<ValidationFilter<UpdateWishlistRequest>>();

        wishlists.MapDelete("/{id:guid}", DeleteWishlist);

        wishlists.MapPost("/wishes/parse-url", ParseWishUrl);

        wishlists.MapPost("/{id:guid}/wishes", AddWish)
            .AddEndpointFilter<ValidationFilter<AddWishRequest>>();

        wishlists.MapPost("/{id:guid}/wishes/from-catalog", AddWishFromCatalog);

        wishlists.MapPut("/{id:guid}/wishes/{wishId:guid}", UpdateWish)
            .AddEndpointFilter<ValidationFilter<UpdateWishRequest>>();

        wishlists.MapDelete("/{id:guid}/wishes/{wishId:guid}", DeleteWish);

        wishlists.MapGet("/{id:guid}/wishes/{wishId:guid}", GetWish);

        wishlists.MapGet("/{id:guid}/wishes", GetWishes);

        wishlists.MapPatch("/{id:guid}/wishes/{wishId:guid}/fulfill", FulfillWish);

        wishlists.MapPatch("/{id:guid}/wishes/{wishId:guid}/unfulfill", UnfulfillWish);

        wishlists.MapPost("/{id:guid}/wishes/{wishId:guid}/duplicate", DuplicateWish);

        wishlists.MapPost("/{id:guid}/wishes/{wishId:guid}/copy", CopyWish);

        wishlists.MapPost("/{id:guid}/wishes/{wishId:guid}/image", UploadWishImage)
            .DisableAntiforgery();;
        
        wishlists.MapDelete("/{id:guid}/wishes/{wishId:guid}/image", DeleteWishImage);
        
        wishlists.MapPost("/{id:guid}/members", AddMembers);
        
        wishlists.MapDelete("/{id:guid}/members/{userId:guid}", RemoveMember);
        
        wishlists.MapGet("/{id:guid}/members", GetMembers);
        
        wishlists.MapPut("/{id:guid}/members/{userId:guid}/role", UpdateMemberRole);
        
        wishlists.MapGet("/{id:guid}/qr", GetWishlistQr)
            .AllowAnonymous();
        
        wishlists.MapGet("/{id:guid}/wishes/{wishId:guid}/qr", GetWishQr)
            .AllowAnonymous();

        return app;
    }
}
