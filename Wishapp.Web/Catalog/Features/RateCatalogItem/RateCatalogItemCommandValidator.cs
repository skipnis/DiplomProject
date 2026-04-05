using FluentValidation;

namespace Wishapp.Web.Catalog.Features.RateCatalogItem;

public sealed class RateCatalogItemRequest
{
    public int Value { get; init; }
}

public sealed class RateCatalogItemRequestValidator : AbstractValidator<RateCatalogItemRequest>
{
    public RateCatalogItemRequestValidator()
    {
        RuleFor(x => x.Value).InclusiveBetween(1, 5);
    }
}
