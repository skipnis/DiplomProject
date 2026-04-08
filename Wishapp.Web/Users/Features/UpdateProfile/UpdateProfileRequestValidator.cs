using FluentValidation;

namespace Wishapp.Web.Users.Features.UpdateProfile;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username может содержать только латинские буквы, цифры и подчёркивание");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio is not null);
    }
}
