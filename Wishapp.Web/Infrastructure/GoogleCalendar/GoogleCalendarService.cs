using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Authentication;

namespace Wishapp.Web.Infrastructure.GoogleCalendar;

public sealed class GoogleCalendarService(
    HttpClient httpClient,
    IOptions<GoogleOptions> googleOptions) : IGoogleCalendarService
{
    private const string CalendarBaseUrl = "https://www.googleapis.com/calendar/v3/calendars/primary/events";
    private const string TokenUrl = "https://oauth2.googleapis.com/token";

    public async Task<Result<string>> ExchangeCodeAsync(string code, CancellationToken ct = default)
    {
        var opts = googleOptions.Value;

        var response = await httpClient.PostAsync(TokenUrl, new FormUrlEncodedContent(
        [
            new("code", code),
            new("client_id", opts.ClientId),
            new("client_secret", opts.ClientSecret),
            new("redirect_uri", "postmessage"),
            new("grant_type", "authorization_code"),
        ]), ct);

        if (!response.IsSuccessStatusCode)
            return Error.Failure("GoogleCalendar.ExchangeFailed", "Failed to exchange authorization code");

        var result = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct);

        if (result?.RefreshToken is null)
            return Error.Failure("GoogleCalendar.NoRefreshToken", "No refresh token returned. Ensure offline access was requested.");

        return result.RefreshToken;
    }

    public async Task<Result<string>> GetAccessTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var opts = googleOptions.Value;

        var response = await httpClient.PostAsync(TokenUrl, new FormUrlEncodedContent(
        [
            new("refresh_token", refreshToken),
            new("client_id", opts.ClientId),
            new("client_secret", opts.ClientSecret),
            new("grant_type", "refresh_token"),
        ]), ct);

        if (!response.IsSuccessStatusCode)
            return Error.Failure("GoogleCalendar.RefreshFailed", "Failed to refresh access token");

        var result = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct);

        return result!.AccessToken;
    }

    public async Task<Result<string>> CreateEventAsync(
        string accessToken,
        GoogleCalendarEventData data,
        CancellationToken ct = default)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var dateStr = data.Date.ToString("yyyy-MM-dd");

        var body = new
        {
            summary = data.Title,
            description = data.Description,
            start = new { date = dateStr },
            end = new { date = dateStr }
        };

        var response = await httpClient.PostAsJsonAsync(CalendarBaseUrl, body, ct);

        if (!response.IsSuccessStatusCode)
            return Error.Failure("GoogleCalendar.CreateFailed", "Failed to create Google Calendar event");

        var result = await response.Content.ReadFromJsonAsync<GoogleCalendarEventResponse>(ct);

        return result!.Id;
    }

    public async Task<Result> UpdateEventAsync(
        string accessToken,
        string eventId,
        GoogleCalendarEventData data,
        CancellationToken ct = default)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var dateStr = data.Date.ToString("yyyy-MM-dd");

        var body = new
        {
            summary = data.Title,
            description = data.Description,
            start = new { date = dateStr },
            end = new { date = dateStr }
        };

        var response = await httpClient.PutAsJsonAsync($"{CalendarBaseUrl}/{eventId}", body, ct);

        return !response.IsSuccessStatusCode
            ? Error.Failure("GoogleCalendar.UpdateFailed", "Failed to update Google Calendar event")
            : Result.Success();
    }

    private record GoogleTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken);

    private record GoogleCalendarEventResponse(
        [property: JsonPropertyName("id")] string Id);
}
