using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Catalog.Dtos;

namespace Wishapp.Web.Admin.Features.Categories.GetAll;

public record GetAllCategoriesQuery : IQuery<List<CatalogCategoryDto>>;
