namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions;

public record CatalogBadgeDefinitionRequest(
    string Emoji,
    string Slug,
    string Label,
    string Description,
    bool IsActive = true);
