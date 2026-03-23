using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.GetAll;

public record GetAllCollectionsQuery : IQuery<List<CatalogCollectionAdminDto>>;
