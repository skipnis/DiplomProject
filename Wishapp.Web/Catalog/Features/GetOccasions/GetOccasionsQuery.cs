using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog.Features.GetOccasions;

public record GetOccasionsQuery : IQuery<List<OccasionDto>>;
