using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Categories.Create;

public record CreateCategoryCommand(string Name, int Order) : ICommand<Guid>;
