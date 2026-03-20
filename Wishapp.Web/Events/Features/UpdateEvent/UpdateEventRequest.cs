namespace Wishapp.Web.Events.Features.UpdateEvent;

public record UpdateEventRequest(string Title, string? Description, DateOnly Date);
