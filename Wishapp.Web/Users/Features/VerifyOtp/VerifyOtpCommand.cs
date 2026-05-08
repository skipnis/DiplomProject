using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.VerifyOtp;

public record VerifyOtpCommand(string Email, string Code, bool RememberMe = false) : ICommand<VerifyOtpResponse>;
