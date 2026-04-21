using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.SetPublished;

public record SetCollectionPublishedCommand(Guid CollectionId, bool IsPublished) : ICommand;
