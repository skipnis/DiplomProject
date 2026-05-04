namespace Wishapp.Web.Users.Entities;

public sealed class UserExternalToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Provider { get; private set; } = null!;
    public string Scope { get; private set; } = null!;
    public string RefreshToken { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private UserExternalToken() { }

    public static UserExternalToken Create(Guid userId, string provider, string scope, string refreshToken)
    {
        return new UserExternalToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Provider = provider,
            Scope = scope,
            RefreshToken = refreshToken,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateRefreshToken(string token) => RefreshToken = token;
}
