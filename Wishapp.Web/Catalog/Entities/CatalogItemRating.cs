namespace Wishapp.Web.Catalog.Entities;

public sealed class CatalogItemRating
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CatalogItemId { get; private set; }
    public int Value { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private CatalogItemRating() { }

    public static CatalogItemRating Create(Guid userId, Guid catalogItemId, int value)
    {
        return new CatalogItemRating
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            CatalogItemId = catalogItemId,
            Value = value,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(int value)
    {
        Value = value;
    }
}
