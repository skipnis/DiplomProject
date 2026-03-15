using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Users;

public interface IUsersApi
{
    public Task<Result> ExistsAsync(Guid userId, CancellationToken ct = default);
}
