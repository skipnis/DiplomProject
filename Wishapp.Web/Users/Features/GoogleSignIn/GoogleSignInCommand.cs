using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.GoogleSignIn;

public record GoogleSignInCommand(string IdToken, bool RememberMe = false) : ICommand<GoogleSignInResponse>;