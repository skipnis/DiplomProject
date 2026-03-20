namespace Wishapp.Web.Users.Entities;

public class UserExternalToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Provider { get; set; }
    public required string Scope { get; set; }
    public required string RefreshToken { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
