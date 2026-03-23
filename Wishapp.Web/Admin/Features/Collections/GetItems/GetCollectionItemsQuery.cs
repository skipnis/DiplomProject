using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.GetItems;

public record GetCollectionItemsQuery(Guid CollectionId) : IQuery<List<CatalogItemDto>>;
