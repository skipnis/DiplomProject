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
    public bool IsFulfilled { get; private set; }
    public DateTimeOffset? FulfilledAt { get; private set; }

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
            CreatedAt = DateTimeOffset.UtcNow
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
    }

    public void SetImage(string imagePath)
    {
        ImagePath = imagePath;
    }

    public void RemoveImage()
    {
        ImagePath = null;
    }

    public void Fulfill()
    {
        IsFulfilled = true;
        FulfilledAt = DateTimeOffset.UtcNow;
    }

    public void Unfulfill()
    {
        IsFulfilled = false;
        FulfilledAt = null;
    }

    public Wish Duplicate(Guid wishlistId)
    {
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
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public Wish CopyTo(Guid targetWishlistId)
    {
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
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}