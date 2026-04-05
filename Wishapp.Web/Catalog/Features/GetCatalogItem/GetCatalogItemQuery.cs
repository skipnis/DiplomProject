using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog.Features.GetCatalogItem;

public record GetCatalogItemQuery(Guid Id, Guid? UserId) : IQuery<CatalogItemDto>;
