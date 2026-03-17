using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Reservations.Dtos;

namespace Wishapp.Web.Reservations.Features.GetMyReservations;

public record GetMyReservationsQuery(Guid UserId, PagedRequest Request)
    : IQuery<PagedResponse<MyReservationDto>>;
