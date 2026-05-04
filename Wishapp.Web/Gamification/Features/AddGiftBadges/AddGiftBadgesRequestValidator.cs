using FluentValidation;

namespace Wishapp.Web.Gamification.Features.AddGiftBadges;

public sealed class AddGiftBadgesRequestValidator : AbstractValidator<AddGiftBadgesRequest>
{
    public AddGiftBadgesRequestValidator()
    {
        RuleFor(x => x.BadgeTypes)
            .NotEmpty()
            .Must(types => types.Count <= 3)
            .WithMessage("Cannot give more than 3 badges per wish")
            .Must(types => types.Distinct().Count() == types.Count)
            .WithMessage("Badge types must be unique");
    }
}
