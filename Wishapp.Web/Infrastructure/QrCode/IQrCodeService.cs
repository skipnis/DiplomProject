namespace Wishapp.Web.Infrastructure.QrCode;

public interface IQrCodeService
{
    byte[] Generate(string url);
}
