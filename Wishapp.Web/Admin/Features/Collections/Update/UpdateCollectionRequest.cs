namespace Wishapp.Web.Admin.Features.Collections.Update;

public record UpdateCollectionRequest(
    string Name,
    string? Description,
    Guid? OccasionId,
    string? CoverImagePath,
    int Order,
    bool IsPublished);
