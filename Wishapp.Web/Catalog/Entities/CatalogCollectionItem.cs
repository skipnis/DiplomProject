namespace Wishapp.Web.Catalog.Entities;

public sealed class CatalogCollectionItem
{
    public Guid CollectionId { get; private set; }
    public Guid CatalogItemId { get; private set; }

    public string? Description { get; private set; }

    public CatalogCollection Collection { get; private set; } = null!;
    public CatalogItem CatalogItem { get; private set; } = null!;

    private CatalogCollectionItem() { }

    public static CatalogCollectionItem Create(Guid collectionId, Guid catalogItemId, string? description)
    {
        return new CatalogCollectionItem
        {
            CollectionId = collectionId,
            CatalogItemId = catalogItemId,
            Description = description
        };
    }
}
