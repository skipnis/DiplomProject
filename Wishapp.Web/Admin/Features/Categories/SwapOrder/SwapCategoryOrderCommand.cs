using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Categories.SwapOrder;

public record SwapCategoryOrderCommand(Guid Id, Guid TargetId) : ICommand;
