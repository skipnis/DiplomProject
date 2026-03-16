namespace Wishapp.Web.Wishlists;

public static partial class WishlistsEndpoints
{
    public static IEndpointRouteBuilder MapWishlistsEndpoints(this IEndpointRouteBuilder app)
    {
        var wishlists = app.MapGroup("/wishlists").RequireAuthorization();

        wishlists.MapPost("/", CreateWishlist);

        wishlists.MapGet("/{id:guid}", GetWishlist).AllowAnonymous();

        wishlists.MapGet("/", GetMyWishlists);

        wishlists.MapPut("/{id:guid}", UpdateWishlist);

        wishlists.MapDelete("/{id:guid}", DeleteWishlist);

        wishlists.MapPost("/wishes/parse-url", ParseWishUrl);

        wishlists.MapPost("/{id:guid}/wishes", AddWish);

        wishlists.MapPut("/{id:guid}/wishes/{wishId:guid}", UpdateWish);

        wishlists.MapDelete("/{id:guid}/wishes/{wishId:guid}", DeleteWish);

        wishlists.MapGet("/{id:guid}/wishes/{wishId:guid}", GetWish);

        wishlists.MapGet("/{id:guid}/wishes", GetWishes);

        wishlists.MapPost("/{id:guid}/wishes/{wishId:guid}/duplicate", DuplicateWish);

        wishlists.MapPost("/{id:guid}/wishes/{wishId:guid}/copy", CopyWish);

        wishlists.MapPost("/{id:guid}/wishes/{wishId:guid}/image", UploadWishImage).DisableAntiforgery();;
        
        wishlists.MapDelete("/{id:guid}/wishes/{wishId:guid}/image", DeleteWishImage);
        
        wishlists.MapPost("/{id:guid}/members", AddMembers);
        
        wishlists.MapDelete("/{id:guid}/members/{userId:guid}", RemoveMember);
        
        wishlists.MapGet("/{id:guid}/members", GetMembers);
        
        wishlists.MapPut("/{id:guid}/members/{userId:guid}/role", UpdateMemberRole);

        return app;
    }
}
