namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions;

public record FulfilledBadgeDefinitionRequest(
    string Emoji,
    string Slug,
    string Label,
    string Description,
    bool IsActive = true);
