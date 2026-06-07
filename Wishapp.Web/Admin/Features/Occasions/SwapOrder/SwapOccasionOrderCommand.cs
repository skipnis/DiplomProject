using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Occasions.SwapOrder;

public record SwapOccasionOrderCommand(Guid Id, Guid TargetId) : ICommand;
