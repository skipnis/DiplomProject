namespace Wishapp.Web.Admin.Entities;

public sealed class AdminUser
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private AdminUser() { }

    public static AdminUser Create(string username, string passwordHash)
    {
        return new AdminUser
        {
            Id = Guid.CreateVersion7(),
            Username = username,
            PasswordHash = passwordHash,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
