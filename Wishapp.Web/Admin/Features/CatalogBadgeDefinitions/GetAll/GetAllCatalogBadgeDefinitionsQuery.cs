using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Features.GetCatalogBadgeDefinitions;

namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.GetAll;

public record GetAllCatalogBadgeDefinitionsQuery : IQuery<List<CatalogBadgeDefinitionDto>>;
