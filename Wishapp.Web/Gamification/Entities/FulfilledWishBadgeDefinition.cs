namespace Wishapp.Web.Gamification.Entities;

public sealed class FulfilledWishBadgeDefinition
{
    public int Id { get; private set; }
    public string Emoji { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private FulfilledWishBadgeDefinition() { }

    public static FulfilledWishBadgeDefinition Create(string emoji, string slug, string label, string description)
    {
        return new FulfilledWishBadgeDefinition { Emoji = emoji, Slug = slug, Label = label, Description = description, IsActive = true };
    }

    public void Update(string emoji, string slug, string label, string description, bool isActive)
    {
        Emoji = emoji;
        Slug = slug;
        Label = label;
        Description = description;
        IsActive = isActive;
    }
}
