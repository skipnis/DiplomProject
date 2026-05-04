namespace Wishapp.Web.Users.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? Username { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? AvatarPath { get; private set; }
    public string? Bio { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsOnboarded { get; private set; }
    public IReadOnlyCollection<AuthIdentity> Identities { get; private set; } = null!;

    private User() { }

    public static User Create(string displayName, string email, string? avatarUrl)
    {
        return new User
        {
            Id = Guid.CreateVersion7(),
            DisplayName = displayName,
            Email = email,
            AvatarUrl = avatarUrl,
            CreatedAt = DateTimeOffset.UtcNow,
            IsOnboarded = false
        };
    }

    public void UpdateProfile(string displayName, string? username, string? bio, DateOnly? birthDate)
    {
        DisplayName = displayName;
        Username = username;
        Bio = bio;
        BirthDate = birthDate;
    }

    public void CompleteOnboarding() => IsOnboarded = true;

    public void SetAvatar(string path) => AvatarPath = path;

    public void RemoveAvatar() => AvatarPath = null;
}
