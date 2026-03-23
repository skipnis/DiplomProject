using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Items.Delete;

public record DeleteCatalogItemCommand(Guid Id) : ICommand;
