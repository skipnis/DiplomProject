using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog.Features.GetCollections;

public record GetCollectionsQuery : IQuery<List<CatalogCollectionSummaryDto>>;
