using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.Create;

public record CreateCollectionCommand(
    string Name,
    string? Description,
    Guid? OccasionId,
    string? CoverImagePath) : ICommand<Guid>;
