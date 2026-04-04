using FluentValidation;

namespace Wishapp.Web.Admin.Features.Collections.Create;

public sealed class CreateCollectionCommandValidator : AbstractValidator<CreateCollectionCommand>
{
    public CreateCollectionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        RuleFor(x => x.Occasion)
            .MaximumLength(50)
            .When(x => x.Occasion is not null);

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0);
    }
}
