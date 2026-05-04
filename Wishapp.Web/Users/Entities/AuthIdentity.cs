namespace Wishapp.Web.Users.Entities;

public sealed class AuthIdentity
{
    public long Id { get; private set; }
    public Guid UserId { get; private set; }
    public string ProviderKey { get; private set; } = null!;
    public AuthProvider Provider { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AuthIdentity() { }

    public static AuthIdentity Create(Guid userId, AuthProvider provider, string providerKey)
    {
        return new AuthIdentity
        {
            UserId = userId,
            Provider = provider,
            ProviderKey = providerKey,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
