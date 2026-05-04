namespace Wishapp.Web.Users.Entities;

public sealed class EmailOtp
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string CodeHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsUsed => UsedAt.HasValue;
    public bool IsValid => !IsExpired && !IsUsed && AttemptCount < 5;

    private EmailOtp() { }

    public static EmailOtp Create(string email, string codeHash)
    {
        return new EmailOtp
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            CodeHash = codeHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void IncrementAttempt() => AttemptCount++;

    public void MarkUsed() => UsedAt = DateTime.UtcNow;
}
