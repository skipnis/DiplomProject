namespace Wishapp.Web.Users.Entities;

public class EmailOtp
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string CodeHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsUsed => UsedAt.HasValue;
    public bool IsValid => !IsExpired && !IsUsed && AttemptCount < 5;

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
}
