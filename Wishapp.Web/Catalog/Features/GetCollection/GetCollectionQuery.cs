using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog.Features.GetCollection;

public record GetCollectionQuery(Guid Id) : IQuery<CatalogCollectionDto>;
