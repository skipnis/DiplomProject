namespace Wishapp.Web.Events;

public interface IEventsApi
{
    Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default);
}
