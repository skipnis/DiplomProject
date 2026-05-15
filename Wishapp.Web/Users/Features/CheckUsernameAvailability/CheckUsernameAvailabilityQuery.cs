using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.CheckUsernameAvailability;

public record CheckUsernameAvailabilityQuery(Guid RequestingUserId, string Username) : IQuery<bool>;
