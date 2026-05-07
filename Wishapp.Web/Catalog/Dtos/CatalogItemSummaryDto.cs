using Wishapp.Web.Gamification.Dtos;

namespace Wishapp.Web.Catalog.Dtos;

public record CatalogItemSummaryDto(
    Guid Id,
    string Name,
    decimal? Price,
    string? Currency,
    string? ImagePath,
    string? Url,
    Guid CategoryId,
    string CategoryName,
    int WishCount,
    string? CollectionItemDescription,
    IReadOnlyList<CatalogItemBadgeDto> Badges);
