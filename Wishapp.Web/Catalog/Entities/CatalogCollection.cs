namespace Wishapp.Web.Catalog.Entities;

public sealed class CatalogCollection
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? OccasionId { get; private set; }
    public CatalogOccasion? Occasion { get; private set; }
    public string? CoverImagePath { get; private set; }
    public int Order { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<CatalogCollectionItem> _items = [];
    public IReadOnlyList<CatalogCollectionItem> Items => _items.AsReadOnly();

    private CatalogCollection() { }

    public static CatalogCollection Create(
        string name,
        string? description,
        Guid? occasionId,
        string? coverImagePath,
        int order)
    {
        return new CatalogCollection
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            OccasionId = occasionId,
            CoverImagePath = coverImagePath,
            Order = order,
            IsPublished = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(
        string name,
        string? description,
        Guid? occasionId,
        string? coverImagePath,
        bool isPublished)
    {
        Name = name;
        Description = description;
        OccasionId = occasionId;
        CoverImagePath = coverImagePath;
        IsPublished = isPublished;
    }

    public void SetOrder(int order)
    {
        Order = order;
    }

    public void SetPublished(bool isPublished)
    {
        IsPublished = isPublished;
    }

    public void SetCoverImage(string? coverImagePath)
    {
        CoverImagePath = coverImagePath;
    }
}
