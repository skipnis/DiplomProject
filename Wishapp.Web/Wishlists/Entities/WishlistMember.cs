namespace Wishapp.Web.Wishlists.Entities;

public sealed class WishlistMember
{
    public Guid Id { get; private set; }
    public Guid WishlistId { get; private set; }
    public Guid UserId { get; private set; }
    public WishlistMemberRole Role { get; private set; }
    public string? CustomRoleName { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    private WishlistMember() { }

    public static WishlistMember Create(Guid wishlistId, Guid userId, WishlistMemberRole role)
    {
        return new WishlistMember
        {
            Id = Guid.CreateVersion7(),
            WishlistId = wishlistId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateRole(WishlistMemberRole role, string? customRoleName)
    {
        Role = role;
        CustomRoleName = customRoleName;
    }

    public void UpdateAlias(string? alias)
    {
        CustomRoleName = alias;
    }
}