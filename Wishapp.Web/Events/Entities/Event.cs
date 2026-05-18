namespace Wishapp.Web.Events.Entities;

public sealed class Event
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateOnly Date { get; private set; }
    public Guid? LinkedWishlistId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Event() { }

    public static Event Create(Guid ownerId, string title, string? description, DateOnly date)
    {
        return new Event
        {
            Id = Guid.CreateVersion7(),
            OwnerId = ownerId,
            Title = title,
            Description = description,
            Date = date,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string title, string? description, DateOnly date)
    {
        Title = title;
        Description = description;
        Date = date;
    }

    public void LinkWishlist(Guid wishlistId) => LinkedWishlistId = wishlistId;

    public void UnlinkWishlist() => LinkedWishlistId = null;
}
