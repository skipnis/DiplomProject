using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog.Features.UnrateCatalogItem;

public record UnrateCatalogItemCommand(Guid UserId, Guid CatalogItemId) : ICommand;
