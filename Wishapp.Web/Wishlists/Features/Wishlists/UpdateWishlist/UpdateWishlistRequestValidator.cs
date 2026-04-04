using FluentValidation;

namespace Wishapp.Web.Wishlists.Features.Wishlists.UpdateWishlist;

public sealed class UpdateWishlistRequestValidator : AbstractValidator<UpdateWishlistRequest>
{
    public UpdateWishlistRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        RuleFor(x => x.Emoji)
            .MaximumLength(10)
            .When(x => x.Emoji is not null);
    }
}
