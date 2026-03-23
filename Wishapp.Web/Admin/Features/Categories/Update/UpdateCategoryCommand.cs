using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Categories.Update;

public record UpdateCategoryCommand(Guid Id, string Name, int Order) : ICommand;
