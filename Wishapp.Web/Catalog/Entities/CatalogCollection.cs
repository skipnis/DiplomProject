namespace Wishapp.Web.Catalog.Entities;

public sealed class CatalogCollection
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Occasion { get; private set; }
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
        string? occasion,
        string? coverImagePath,
        int order)
    {
        return new CatalogCollection
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            Occasion = occasion,
            CoverImagePath = coverImagePath,
            Order = order,
            IsPublished = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(
        string name,
        string? description,
        string? occasion,
        string? coverImagePath,
        int order,
        bool isPublished)
    {
        Name = name;
        Description = description;
        Occasion = occasion;
        CoverImagePath = coverImagePath;
        Order = order;
        IsPublished = isPublished;
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
