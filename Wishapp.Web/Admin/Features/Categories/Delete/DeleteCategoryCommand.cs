using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Categories.Delete;

public record DeleteCategoryCommand(Guid Id) : ICommand;
