using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.SendOtp;

public record SendOtpCommand(string Email) : ICommand;
