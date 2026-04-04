using FluentValidation;

namespace Wishapp.Web.Wishlists.Features.Wishlists.CopyWish;

public sealed class CopyWishRequestValidator : AbstractValidator<CopyWishRequest>
{
    public CopyWishRequestValidator()
    {
        RuleFor(x => x.TargetWishlistId)
            .NotEmpty();
    }
}
