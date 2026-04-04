using FluentValidation;

namespace Wishapp.Web.Catalog.Dtos;

public sealed class CatalogItemsRequestValidator : AbstractValidator<CatalogItemsRequest>
{
    public CatalogItemsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => x.Search is not null);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice is not null);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice is not null);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice!.Value)
            .WithMessage("'MaxPrice' must be greater than or equal to 'MinPrice'.")
            .When(x => x.MinPrice is not null && x.MaxPrice is not null);
    }
}
