namespace Wishapp.Web.Catalog.Entities;

public sealed class CatalogOccasion
{
    public Guid Id { get; private set; }
    public string Key { get; private set; } = null!;
    public string Label { get; private set; } = null!;
    public int Order { get; private set; }

    private CatalogOccasion() { }

    public static CatalogOccasion Create(string key, string label, int order)
    {
        return new CatalogOccasion
        {
            Id = Guid.CreateVersion7(),
            Key = key,
            Label = label,
            Order = order
        };
    }

    public void Update(string key, string label)
    {
        Key = key;
        Label = label;
    }

    public void SetOrder(int order)
    {
        Order = order;
    }
}
