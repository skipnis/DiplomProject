using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.Update;

public record UpdateCatalogBadgeDefinitionCommand(
    int Id,
    string Emoji,
    string Slug,
    string Label,
    string Description,
    bool IsActive) : ICommand;
