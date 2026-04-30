using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.UpdateItemDescription;

public record UpdateCollectionItemDescriptionCommand(Guid CollectionId, Guid CatalogItemId, string? Description) : ICommand;
