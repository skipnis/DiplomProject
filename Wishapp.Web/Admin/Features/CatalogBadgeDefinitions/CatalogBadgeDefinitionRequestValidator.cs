using FluentValidation;

namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions;

public sealed class CatalogBadgeDefinitionRequestValidator : AbstractValidator<CatalogBadgeDefinitionRequest>
{
    public CatalogBadgeDefinitionRequestValidator()
    {
        RuleFor(x => x.Emoji)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}
