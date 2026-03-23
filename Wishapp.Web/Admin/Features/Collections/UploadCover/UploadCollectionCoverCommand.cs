using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Collections.UploadCover;

public record UploadCollectionCoverCommand(
    Guid CollectionId,
    IFormFile File) : ICommand<UploadCollectionCoverResponse>;
