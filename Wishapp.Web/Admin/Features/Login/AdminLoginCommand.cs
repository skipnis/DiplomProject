using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Login;

public record AdminLoginCommand(string Username, string Password) : ICommand<AdminLoginResponse>;
