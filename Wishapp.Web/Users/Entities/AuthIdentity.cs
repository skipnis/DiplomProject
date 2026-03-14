namespace Wishapp.Web.Users.Entities;

public class AuthIdentity
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public required string ProviderKey { get; set; }
    public AuthProvider Provider { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
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

