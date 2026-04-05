using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog.Features.RateCatalogItem;

public record RateCatalogItemCommand(Guid UserId, Guid CatalogItemId, int Value) : ICommand;
