namespace Wishapp.Web.Admin.Features.GetStats;

public record AdminStatsResponse(
    AdminUserStats Users,
    AdminContentStats Content,
    AdminActivityStats Activity,
    AdminCatalogStats Catalog);

public record AdminUserStats(
    int Total,
    int NewLast7Days,
    int NewLast30Days);

public record AdminContentStats(
    int TotalWishlists,
    int TotalWishes,
    double AverageWishesPerWishlist,
    int WishesWithImage,
    int WishesWithoutImage);

public record AdminActivityStats(
    int ActiveReservations,
    int FulfilledWishes,
    int FulfilledWithGifter,
    List<TopGifterDto> TopGifters);

public record TopGifterDto(
    Guid UserId,
    string DisplayName,
    int FulfilledCount);

public record AdminCatalogStats(
    List<TopCatalogItemDto> TopItems);

public record TopCatalogItemDto(
    Guid Id,
    string Name,
    int WishCount);
