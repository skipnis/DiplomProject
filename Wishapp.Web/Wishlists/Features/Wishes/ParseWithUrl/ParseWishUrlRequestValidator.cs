using FluentValidation;

namespace Wishapp.Web.Wishlists.Features.Wishes.ParseWithUrl;

public sealed class ParseWishUrlRequestValidator : AbstractValidator<ParseWishUrlRequest>
{
    public ParseWishUrlRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("'Url' must be a valid absolute URL.");
    }
}
