using FluentValidation;

namespace Wishapp.Web.Admin.Features.Login;

public sealed class AdminLoginCommandValidator : AbstractValidator<AdminLoginCommand>
{
    public AdminLoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(200);
    }
}
