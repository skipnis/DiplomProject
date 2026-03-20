using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Events.Dtos;

namespace Wishapp.Web.Events.Features.GetEvent;

public record GetEventQuery(Guid EventId, Guid UserId) : IQuery<EventDto>;
