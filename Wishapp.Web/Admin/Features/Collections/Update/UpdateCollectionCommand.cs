using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.Update;

public record UpdateCollectionCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Occasion,
    string? CoverImagePath,
    int Order,
    bool IsPublished) : ICommand;
