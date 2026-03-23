using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.RemoveItem;

public record RemoveItemFromCollectionCommand(Guid CollectionId, Guid CatalogItemId) : ICommand;
