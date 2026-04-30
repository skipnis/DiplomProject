using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Dtos;

public record WishDto(
    Guid Id,
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    WishPriority Priority,
    string? Url,
    string? ImagePath,
    DateTimeOffset CreatedAt,
    bool IsFulfilled,
    DateTimeOffset? FulfilledAt,
    bool IsReserved,
    Guid? ShareToken,
    Guid? FulfilledByReserverId,
    bool HasGiftBadges);
