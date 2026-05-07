using FluentValidation;

namespace Wishapp.Web.Admin.Features.Collections.Update;

public sealed class UpdateCollectionRequestValidator : AbstractValidator<UpdateCollectionRequest>
{
    public UpdateCollectionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);


        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0);
    }
}
