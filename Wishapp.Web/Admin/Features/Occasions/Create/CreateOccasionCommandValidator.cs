using FluentValidation;

namespace Wishapp.Web.Admin.Features.Occasions.Create;

public sealed class CreateOccasionCommandValidator : AbstractValidator<CreateOccasionCommand>
{
    public CreateOccasionCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(100);

    }
}
