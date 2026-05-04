using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.Delete;

public record DeleteCatalogBadgeDefinitionCommand(int Id) : ICommand;
