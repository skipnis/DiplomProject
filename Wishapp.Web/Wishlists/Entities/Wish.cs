using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Wishlists.Entities;

public sealed class Wish
{
    public Guid Id { get; private set; }
    public Guid WishlistId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal? Price { get; private set; }
    public Currency? Currency { get; private set; }
    public WishPriority Priority { get; private set; }
    public string? Url { get; private set; }
    public string? ImagePath { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsFulfilled { get; private set; }
    public DateTimeOffset? FulfilledAt { get; private set; }
    public Guid? FulfilledByUserId { get; private set; }
    public Guid? FulfilledByReserverId { get; private set; }
    public Guid ShareToken { get; private set; }

    private Wish() { }

    public static Wish Create(
        Guid wishlistId,
        string name,
        string? description,
        decimal? price,
        Currency? currency,
        WishPriority priority,
        string? url)
    {
        var now = DateTimeOffset.UtcNow;
        return new Wish
        {
            Id = Guid.CreateVersion7(),
            WishlistId = wishlistId,
            Name = name,
            Description = description,
            Price = price,
            Currency = currency,
            Priority = priority,
            Url = url,
            CreatedAt = now,
            UpdatedAt = now,
            ShareToken = Guid.CreateVersion7()
        };
    }

    public void Update(
        string name,
        string? description,
        decimal? price,
        Currency? currency,
        WishPriority priority,
        string? url)
    {
        Name = name;
        Description = description;
        Price = price;
        Currency = currency;
        Priority = priority;
        Url = url;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetImage(string imagePath)
    {
        ImagePath = imagePath;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveImage()
    {
        ImagePath = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Fulfill(Guid fulfilledByUserId, Guid? reserverId = null)
    {
        IsFulfilled = true;
        FulfilledAt = DateTimeOffset.UtcNow;
        FulfilledByUserId = fulfilledByUserId;
        FulfilledByReserverId = reserverId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unfulfill()
    {
        IsFulfilled = false;
        FulfilledAt = null;
        FulfilledByUserId = null;
        FulfilledByReserverId = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RegenerateShareToken()
    {
        ShareToken = Guid.CreateVersion7();
    }

    public Wish Duplicate(Guid wishlistId)
    {
        var now = DateTimeOffset.UtcNow;
        return new Wish
        {
            Id = Guid.CreateVersion7(),
            WishlistId = wishlistId,
            Name = Name,
            Description = Description,
            Price = Price,
            Currency = Currency,
            Priority = Priority,
            Url = Url,
            ImagePath = null,
            IsFulfilled = false,
            FulfilledAt = null,
            CreatedAt = now,
            UpdatedAt = now,
            ShareToken = Guid.CreateVersion7()
        };
    }

    public Wish CopyTo(Guid targetWishlistId)
    {
        var now = DateTimeOffset.UtcNow;
        return new Wish
        {
            Id = Guid.CreateVersion7(),
            WishlistId = targetWishlistId,
            Name = Name,
            Description = Description,
            Price = Price,
            Currency = Currency,
            Priority = Priority,
            Url = Url,
            ImagePath = null,
            IsFulfilled = false,
            FulfilledAt = null,
            CreatedAt = now,
            UpdatedAt = now,
            ShareToken = Guid.CreateVersion7()
        };
    }
}
