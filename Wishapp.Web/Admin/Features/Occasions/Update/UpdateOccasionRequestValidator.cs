using FluentValidation;

namespace Wishapp.Web.Admin.Features.Occasions.Update;

public sealed class UpdateOccasionRequestValidator : AbstractValidator<UpdateOccasionRequest>
{
    public UpdateOccasionRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(100);

    }
}
