using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Admin.Features.Items.Update;

public record UpdateCatalogItemCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    string? ImagePath,
    string? Url,
    Guid CategoryId,
    bool IsPublished,
    List<Guid> OccasionIds) : ICommand;
