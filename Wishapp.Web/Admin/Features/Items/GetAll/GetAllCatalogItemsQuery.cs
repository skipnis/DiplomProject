using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin.Features.Items.GetAll;

public record GetAllCatalogItemsQuery(CatalogItemsFilter Filter, PagedRequest Request)
    : IQuery<PagedResponse<CatalogItemDto>>;
