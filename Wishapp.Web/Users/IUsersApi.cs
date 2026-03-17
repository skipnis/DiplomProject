using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Users;

public interface IUsersApi
{
    public Task<Result> ExistsAsync(Guid userId, CancellationToken ct = default);
    Task<Dictionary<Guid, string>> GetUsernamesAsync(List<Guid> userIds, CancellationToken ct = default);
}
