namespace Wishapp.Web.Catalog.Entities;

public sealed class CatalogItemOccasion
{
    public Guid CatalogItemId { get; private set; }
    public Guid OccasionId { get; private set; }

    public CatalogItem CatalogItem { get; private set; } = null!;
    public CatalogOccasion Occasion { get; private set; } = null!;

    private CatalogItemOccasion() { }

    public static CatalogItemOccasion Create(Guid catalogItemId, Guid occasionId)
    {
        return new CatalogItemOccasion
        {
            CatalogItemId = catalogItemId,
            OccasionId = occasionId
        };
    }
}
