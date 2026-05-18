namespace Wishapp.Web.Events.Dtos;

public record EventDto(
    Guid Id,
    string Title,
    string? Description,
    DateOnly Date,
    Guid? LinkedWishlistId,
    DateTimeOffset CreatedAt);
