namespace Wishapp.Web.Users.Entities;

public sealed class BlacklistItem
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private BlacklistItem() { }

    public static BlacklistItem Create(Guid userId, string title) => new()
    {
        Id = Guid.CreateVersion7(),
        UserId = userId,
        Title = title,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public void UpdateTitle(string title) => Title = title;
}
