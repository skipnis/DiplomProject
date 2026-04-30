using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Users;

public record UserPublicInfo(string DisplayName, string? AvatarUrl);

public interface IUsersApi
{
    Task<Result> ExistsAsync(Guid userId, CancellationToken ct = default);
    Task<List<Guid>> FilterExistingIdsAsync(List<Guid> userIds, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetUsernamesAsync(List<Guid> userIds, CancellationToken ct = default);
    Task<Dictionary<Guid, UserPublicInfo>> GetUsersPublicInfoAsync(List<Guid> userIds, CancellationToken ct = default);
    Task<Result<string>> GetExternalRefreshTokenAsync(Guid userId, string provider, string scope, CancellationToken ct = default);
}
