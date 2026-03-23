using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Catalog.Features.GetCatalogItems;

public record GetCatalogItemsQuery(CatalogItemsFilter Filter, PagedRequest Request)
    : IQuery<PagedResponse<CatalogItemDto>>;
