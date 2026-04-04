using FluentValidation;

namespace Wishapp.Web.Events.Features.LinkWishlist;

public sealed class LinkWishlistRequestValidator : AbstractValidator<LinkWishlistRequest>
{
    public LinkWishlistRequestValidator()
    {
        RuleFor(x => x.WishlistId)
            .NotEmpty()
            .When(x => x.WishlistId.HasValue);
    }
}
