namespace Wishapp.Web.Proposals;

public interface IProposalsApi
{
    Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default);
}
