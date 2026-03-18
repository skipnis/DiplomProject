using System.Net;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Infrastructure.Parser;

public static class UrlValidator
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    public static Result ValidatePageUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return Error.Validation("Url.Invalid", "Invalid URL format");
        }

        if (uri.Scheme != "https" && uri.Scheme != "http")
        {
            return Error.Validation("Url.InvalidScheme", "URL must be http or https");
        }

        if (IsPrivateHost(uri.Host))
        {
            return Error.Validation("Url.PrivateHost", "Private IP addresses are not allowed");
        }

        return Result.Success();
    }

    public static Result ValidateImageUrl(string url)
    {
        return ValidatePageUrl(url);
    }

    private static bool IsPrivateHost(string host)
    {
        if (host == "localhost")
            return true;

        if (!IPAddress.TryParse(host, out var ip))
            return false;

        var bytes = ip.GetAddressBytes();

        return bytes[0] == 10 ||
               bytes[0] == 127 ||
               (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
               (bytes[0] == 192 && bytes[1] == 168);
    }
}