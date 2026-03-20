using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Infrastructure.GoogleCalendar;

public interface IGoogleCalendarService
{
    Task<Result<string>> ExchangeCodeAsync(string code, CancellationToken ct = default);

    Task<Result<string>> GetAccessTokenAsync(string refreshToken, CancellationToken ct = default);

    Task<Result<string>> CreateEventAsync(string accessToken, GoogleCalendarEventData data, CancellationToken ct = default);

    Task<Result> UpdateEventAsync(string accessToken, string eventId, GoogleCalendarEventData data, CancellationToken ct = default);
}
