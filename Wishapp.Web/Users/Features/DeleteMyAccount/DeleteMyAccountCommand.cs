using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.DeleteMyAccount;

public record DeleteMyAccountCommand(Guid UserId) : ICommand;
