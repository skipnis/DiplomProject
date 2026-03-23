using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.AddItem;

public record AddItemToCollectionCommand(Guid CollectionId, Guid CatalogItemId) : ICommand;
