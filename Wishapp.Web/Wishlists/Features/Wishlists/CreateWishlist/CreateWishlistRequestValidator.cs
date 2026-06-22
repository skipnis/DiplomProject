using FluentValidation;

namespace Wishapp.Web.Wishlists.Features.Wishlists.CreateWishlist;

public sealed class CreateWishlistRequestValidator : AbstractValidator<CreateWishlistRequest>
{
    public CreateWishlistRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description);

        RuleFor(x => x.Emoji)
            .MaximumLength(10)
            .When(x => x.Emoji is not null);
    }
}
