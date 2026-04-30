namespace Wishapp.Web.Gamification;

public static partial class GamificationEndpoints
{
    public static IEndpointRouteBuilder MapGamificationEndpoints(this IEndpointRouteBuilder app)
    {
        var catalog = app.MapGroup("/catalog");
        catalog.MapPost("/items/{id:guid}/badges/{badgeType}", VoteCatalogItemBadge)
            .RequireAuthorization();
        catalog.MapDelete("/items/{id:guid}/badges/{badgeType}", UnvoteCatalogItemBadge)
            .RequireAuthorization();
        catalog.MapGet("/badge-definitions", GetBadgeDefinitions);
        catalog.MapGet("/fulfilled-badge-definitions", GetFulfilledBadgeDefinitions);

        var wishlists = app.MapGroup("/wishlists").RequireAuthorization();
        wishlists.MapPost("/{id:guid}/wishes/{wishId:guid}/gift-badges", AddGiftBadges);
        wishlists.MapGet("/{id:guid}/wishes/{wishId:guid}/gift-badges", GetGiftBadges);

        var users = app.MapGroup("/users").RequireAuthorization();
        users.MapGet("/me/gift-profile", GetMyGiftProfile);
        users.MapGet("/{id:guid}/gift-profile", GetUserGiftProfile).AllowAnonymous();

        return app;
    }
}
