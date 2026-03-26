using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResponse>;
