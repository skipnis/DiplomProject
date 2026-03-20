namespace Wishapp.Web.Events.Entities;

public class Event
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public string? GoogleCalendarEventId { get; set; }
    public Guid? LinkedWishlistId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

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
}
