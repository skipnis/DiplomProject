using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Items.Create;

public record CreateCatalogItemCommand(
    string Name,
    string? Description,
    decimal? Price,
    Currency? Currency,
    string? ImagePath,
    string? Url,
    Guid CategoryId,
    List<Guid> OccasionIds) : ICommand<Guid>;
