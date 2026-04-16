using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog.Features.GetPriceRange;

public record GetPriceRangeQuery : IQuery<PriceRangeResult>;

public record PriceRangeResult(int Max);
