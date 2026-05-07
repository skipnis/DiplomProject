using FluentValidation;
using Wishapp.Web.Common;

namespace Wishapp.Web.Wishlists.Features.Wishes.UploadWishImage;

public sealed class UploadWishImageRequestValidator : AbstractValidator<UploadWishImageRequest>
{
    public UploadWishImageRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.File is not null || x.ExternalImageUrl is not null)
            .WithMessage("File or ExternalImageUrl must be provided.")
            .Must(x => !(x.File is not null && x.ExternalImageUrl is not null))
            .WithMessage("Provide either File or ExternalImageUrl, not both.");

        RuleFor(x => x.File)
            .Must(f => f!.Length <= StorageLimits.MaxImageSizeBytes)
            .WithMessage("Image must be less than 10MB.")
            .When(x => x.File is not null);

        RuleFor(x => x.ExternalImageUrl)
            .MaximumLength(2048)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("'ExternalImageUrl' must be a valid absolute URL.")
            .When(x => x.ExternalImageUrl is not null);
    }
}
