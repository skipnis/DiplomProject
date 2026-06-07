using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.SwapOrder;

public record SwapCollectionOrderCommand(Guid Id, Guid TargetId) : ICommand;
