using System.Collections.Concurrent;
using System.Threading.Channels;
using Wishapp.Web.Notifications.Dtos;

namespace Wishapp.Web.Notifications.Sse;

public sealed class SseConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<NotificationDto>>> _connections = new();

    public (Guid connectionId, Channel<NotificationDto> channel) Register(Guid userId)
    {
        var connectionId = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<NotificationDto>(
            new UnboundedChannelOptions { SingleReader = true });

        _connections.GetOrAdd(userId, _ => new()).TryAdd(connectionId, channel);
        return (connectionId, channel);
    }

    public void Unregister(Guid userId, Guid connectionId)
    {
        if (_connections.TryGetValue(userId, out var userConnections))
            userConnections.TryRemove(connectionId, out _);
    }

    public IEnumerable<Channel<NotificationDto>> GetChannels(Guid userId)
    {
        return _connections.TryGetValue(userId, out var userConnections)
            ? userConnections.Values
            : [];
    }
}
