using FluentValidation;

namespace Wishapp.Web.Users.Features.ConnectGoogleCalendar;

public sealed class ConnectGoogleCalendarRequestValidator : AbstractValidator<ConnectGoogleCalendarRequest>
{
    public ConnectGoogleCalendarRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty();
    }
}
