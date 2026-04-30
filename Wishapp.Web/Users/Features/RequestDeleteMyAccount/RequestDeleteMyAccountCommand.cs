using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.RequestDeleteMyAccount;

public record RequestDeleteMyAccountCommand(Guid UserId) : ICommand;
