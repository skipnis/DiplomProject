namespace Wishapp.Web.Catalog.Entities;

public sealed class CatalogCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private CatalogCategory() { }

    public static CatalogCategory Create(string name, int order)
    {
        return new CatalogCategory
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Order = order,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string name, int order)
    {
        Name = name;
        Order = order;
    }
}
