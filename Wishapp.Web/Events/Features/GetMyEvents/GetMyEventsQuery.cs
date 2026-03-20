using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Events.Dtos;

namespace Wishapp.Web.Events.Features.GetMyEvents;

public record GetMyEventsQuery(Guid UserId) : IQuery<IEnumerable<EventDto>>;
