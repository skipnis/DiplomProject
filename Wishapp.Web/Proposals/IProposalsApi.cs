namespace Wishapp.Web.Proposals;

public interface IProposalsApi
{
    Task<int> GetLikedProposalsCountAsync(Guid senderId, CancellationToken ct = default);
    Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default);
}
