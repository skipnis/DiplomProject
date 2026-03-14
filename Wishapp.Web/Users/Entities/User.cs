namespace Wishapp.Web.Users.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateOnly? BirthDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IReadOnlyCollection<AuthIdentity> Identities { get; set; }
    
    public static User Create(string displayName, string email, string? avatarUrl)
    {
        return new User
        {
            Id = Guid.CreateVersion7(),
            Username = displayName,
            Email = email,
            AvatarUrl = avatarUrl,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}