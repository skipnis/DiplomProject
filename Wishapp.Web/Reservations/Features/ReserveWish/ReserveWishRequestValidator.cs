using FluentValidation;

namespace Wishapp.Web.Reservations.Features.ReserveWish;

public sealed class ReserveWishRequestValidator : AbstractValidator<ReserveWishRequest>
{
    public ReserveWishRequestValidator()
    {
        RuleFor(x => x.WishlistId)
            .NotEmpty();
    }
}
