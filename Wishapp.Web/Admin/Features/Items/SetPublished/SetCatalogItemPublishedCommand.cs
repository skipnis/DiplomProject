using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Items.SetPublished;

public record SetCatalogItemPublishedCommand(Guid ItemId, bool IsPublished) : ICommand;
