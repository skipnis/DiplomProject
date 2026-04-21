using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Categories.SetPublished;

public record SetCategoryPublishedCommand(Guid CategoryId, bool IsPublished) : ICommand;
