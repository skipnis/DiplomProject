namespace Wishapp.Web.Events.Features.CreateEvent;

public record CreateEventRequest(string Title, string? Description, DateOnly Date);
