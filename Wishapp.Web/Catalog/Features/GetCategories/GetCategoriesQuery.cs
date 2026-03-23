using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog.Features.GetCategories;

public record GetCategoriesQuery : IQuery<List<CatalogCategoryDto>>;
