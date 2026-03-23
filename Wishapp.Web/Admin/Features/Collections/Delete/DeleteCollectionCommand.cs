using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.Delete;

public record DeleteCollectionCommand(Guid Id) : ICommand;
