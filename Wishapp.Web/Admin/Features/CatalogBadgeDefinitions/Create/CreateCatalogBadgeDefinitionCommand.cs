using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.Create;

public record CreateCatalogBadgeDefinitionCommand(
    string Emoji,
    string Slug,
    string Label,
    string Description) : ICommand<int>;
