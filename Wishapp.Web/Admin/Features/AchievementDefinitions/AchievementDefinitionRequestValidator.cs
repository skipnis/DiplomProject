using FluentValidation;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions;

public sealed class AchievementDefinitionRequestValidator : AbstractValidator<AchievementDefinitionRequest>
{
    public AchievementDefinitionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Emoji)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Threshold)
            .GreaterThan(0);

    }
}
