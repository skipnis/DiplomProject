using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Catalog.Entities;

public sealed class CatalogItem
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal? Price { get; private set; }
    public Currency? Currency { get; private set; }
    public string? ImagePath { get; private set; }
    public string? Url { get; private set; }
    public Guid CategoryId { get; private set; }
    public CatalogCategory Category { get; private set; } = null!;
    public bool IsPublished { get; private set; }
    public int WishCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CatalogItem() { }

    public static CatalogItem Create(
        string name,
        string? description,
        decimal? price,
        Currency? currency,
        string? imagePath,
        string? url,
        Guid categoryId)
    {
        return new CatalogItem
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            Price = price,
            Currency = currency,
            ImagePath = imagePath,
            Url = url,
            CategoryId = categoryId,
            IsPublished = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(
        string name,
        string? description,
        decimal? price,
        Currency? currency,
        string? imagePath,
        string? url,
        Guid categoryId,
        bool isPublished)
    {
        Name = name;
        Description = description;
        Price = price;
        Currency = currency;
        ImagePath = imagePath;
        Url = url;
        CategoryId = categoryId;
        IsPublished = isPublished;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPublished(bool isPublished)
    {
        IsPublished = isPublished;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementWishCount()
    {
        WishCount++;
    }

    public void SetImage(string? imagePath)
    {
        ImagePath = imagePath;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
