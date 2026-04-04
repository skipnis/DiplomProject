using FluentValidation;

namespace Wishapp.Web.Wishlists.Features.Wishes.AddWish;

public sealed class AddWishRequestValidator : AbstractValidator<AddWishRequest>
{
    public AddWishRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Url)
            .MaximumLength(2048)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("'Url' must be a valid absolute URL.")
            .When(x => x.Url is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price is not null);
    }
}
