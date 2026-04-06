using FluentValidation;

namespace Wishapp.Web.Users.Features.UpdateProfile;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_а-яА-ЯёЁ]+$")
            .WithMessage("Имя пользователя может содержать только буквы, цифры и подчёркивание");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio is not null);
    }
}
