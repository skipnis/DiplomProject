namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions;

public sealed class FulfilledBadgeDefinitionRequest
{
    public string Emoji { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}
