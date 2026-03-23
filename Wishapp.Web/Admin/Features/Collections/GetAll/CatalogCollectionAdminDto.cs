namespace Wishapp.Web.Admin.Features.Collections.GetAll;

public record CatalogCollectionAdminDto(
    Guid Id,
    string Name,
    string? Description,
    string? Occasion,
    string? CoverImagePath,
    int Order,
    bool IsPublished,
    int ItemCount,
    DateTimeOffset CreatedAt);
