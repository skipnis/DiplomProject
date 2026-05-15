using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Wishlists.Entities;

public sealed class Wishlist
{
    private readonly List<WishlistMember> _members = [];
    public IReadOnlyCollection<WishlistMember> Members => _members.AsReadOnly();
    
    private readonly List<Wish> _wishes = [];
    public IReadOnlyCollection<Wish> Wishes => _wishes.AsReadOnly();

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Emoji { get; private set; }
    public WishlistVisibility Visibility { get; private set; }
    public bool IsSystem { get; private set; }
    public SystemWishlistType SystemType { get; private set; }
    public bool IsSurpriseModeEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Wishlist() { }

    public static Wishlist Create(
        Guid ownerId,
        string name,
        string? description,
        string? emoji,
        WishlistVisibility visibility,
        bool isSurpriseModeEnabled = false)
    {
        var wishlist = new Wishlist
        {
            Id = Guid.CreateVersion7(),
            OwnerId = ownerId,
            Name = name,
            Description = description,
            Emoji = emoji,
            Visibility = visibility,
            IsSystem = false,
            IsSurpriseModeEnabled = isSurpriseModeEnabled,
            CreatedAt = DateTimeOffset.UtcNow
        };

        wishlist._members.Add(WishlistMember.Create(
            wishlist.Id, ownerId, WishlistMemberRole.Owner));

        return wishlist;
    }

    public static Wishlist CreateSystem(
        Guid ownerId,
        string name,
        WishlistVisibility visibility,
        SystemWishlistType systemType)
    {
        var wishlist = new Wishlist
        {
            Id = Guid.CreateVersion7(),
            OwnerId = ownerId,
            Name = name,
            Visibility = visibility,
            IsSystem = true,
            SystemType = systemType,
            CreatedAt = DateTimeOffset.UtcNow
        };

        wishlist._members.Add(WishlistMember.Create(
            wishlist.Id, ownerId, WishlistMemberRole.Owner));

        return wishlist;
    }

    public Result Update(string name, string? description, string? emoji, WishlistVisibility visibility)
    {
        if (IsSystem && Visibility != visibility)
        {
            return Error.Failure("Wishlists.SystemVisibility", "Cannot change visibility of system wishlist");
        }

        Name = name;
        
        Description = description;
        
        Emoji = emoji;
        
        Visibility = visibility;

        return Result.Success();
    }

    public Result Delete()
    {
        return IsSystem 
            ? Error.Failure("Wishlists.SystemDelete", "Cannot delete system wishlist")
            : Result.Success();
    }

    public Result<WishlistMember> AddMember(Guid userId, WishlistMemberRole role)
    {
        if (Visibility == WishlistVisibility.Private)
        {
            return Error.Failure("Wishlists.PrivateWishlist", "Cannot add members to private wishlist");
        }

        if (role == WishlistMemberRole.Owner)
        {
            return Error.Failure("Wishlists.InvalidRole", "Cannot add member with Owner role");
        }

        if (_members.Any(m => m.UserId == userId))
        {
            return Error.Conflict("Wishlists.MemberExists", "User is already a member");
        }

        var member = WishlistMember.Create(Id, userId, role);
        _members.Add(member);

        return Result.Success(member);
    }

    public Result RemoveMember(Guid userId)
    {
        if (Visibility == WishlistVisibility.Private)
        {
            return Error.Failure("Wishlists.PrivateWishlist", "Cannot remove members from private wishlist");
        }

        var member = _members.FirstOrDefault(m => m.UserId == userId);

        if (member is null)
        {
            return Error.NotFound("Wishlists.MemberNotFound", "Member not found");
        }

        if (member.Role == WishlistMemberRole.Owner)
        {
            return Error.Failure("Wishlists.OwnerRemove", "Cannot remove Owner from wishlist");
        }

        _members.Remove(member);
        
        return Result.Success();
    }

    public Result UpdateMemberRole(Guid userId, WishlistMemberRole role, string? customRoleName)
    {
        if (role == WishlistMemberRole.Owner)
        {
            return Error.Failure("Wishlists.InvalidRole", "Cannot assign Owner role");
        }

        var member = _members.FirstOrDefault(m => m.UserId == userId);

        if (member is null)
        {
            return Error.NotFound("Wishlists.MemberNotFound", "Member not found");
        }

        if (member.Role == WishlistMemberRole.Owner)
        {
            member.UpdateAlias(customRoleName);
            return Result.Success();
        }

        member.UpdateRole(role, customRoleName);

        return Result.Success();
    }

    public Result<Wish> AddWish(
        string name,
        string? description,
        decimal? price,
        Currency? currency,
        WishPriority priority,
        string? url,
        Guid? createdByUserId = null)
    {
        var wish = Wish.Create(Id, name, description, price, currency, priority, url, createdByUserId);

        _wishes.Add(wish);

        return Result.Success(wish);
    }

    public Result UpdateWish(
        Guid wishId,
        string name,
        string? description,
        decimal? price,
        Currency? currency,
        WishPriority priority,
        string? url)
    {
        var wish = _wishes.FirstOrDefault(w => w.Id == wishId);

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        wish.Update(name, description, price, currency, priority, url);

        return Result.Success();
    }

    public Result RemoveWish(Guid wishId)
    {
        var wish = _wishes.FirstOrDefault(w => w.Id == wishId);

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        _wishes.Remove(wish);

        return Result.Success();
    }

    public Result<Wish> DuplicateWish(Guid wishId, Guid createdByUserId)
    {
        var wish = _wishes.FirstOrDefault(w => w.Id == wishId);

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        var duplicate = wish.Duplicate(Id, createdByUserId);

        _wishes.Add(duplicate);

        return Result.Success(duplicate);
    }

    public Result FulfillWish(Guid wishId, Guid fulfilledByUserId, Guid? reserverId = null)
    {
        var wish = _wishes.FirstOrDefault(w => w.Id == wishId);

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        wish.Fulfill(fulfilledByUserId, reserverId);

        return Result.Success();
    }

    public Result UnfulfillWish(Guid wishId)
    {
        var wish = _wishes.FirstOrDefault(w => w.Id == wishId);

        if (wish is null)
        {
            return Error.NotFound("Wishes.NotFound", "Wish not found");
        }

        wish.Unfulfill();

        return Result.Success();
    }

    public Result<Wish> CopyWishFrom(Wish wish, Guid createdByUserId)
    {
        var copy = wish.CopyTo(Id, createdByUserId);

        _wishes.Add(copy);

        return Result.Success(copy);
    }
}