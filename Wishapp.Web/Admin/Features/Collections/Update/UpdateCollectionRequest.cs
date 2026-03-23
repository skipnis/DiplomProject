namespace Wishapp.Web.Admin.Features.Collections.Update;

public record UpdateCollectionRequest(
    string Name,
    string? Description,
    string? Occasion,
    string? CoverImagePath,
    int Order,
    bool IsPublished);
