namespace Wishapp.Web.Users.Entities;

public sealed class UserRefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;

    private UserRefreshToken() { }

    public static UserRefreshToken Create(Guid userId, string tokenHash, int expirationDays) => new()
    {
        Id = Guid.CreateVersion7(),
        UserId = userId,
        TokenHash = tokenHash,
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
        CreatedAt = DateTime.UtcNow
    };

    public void Revoke() => RevokedAt = DateTime.UtcNow;
}
