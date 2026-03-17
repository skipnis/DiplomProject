using QRCoder;

namespace Wishapp.Web.Infrastructure.QrCode;

public sealed class QrCodeService : IQrCodeService
{
    public byte[] Generate(string url)
    {
        using var qrGenerator = new QRCodeGenerator();
        
        var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        
        using var qrCode = new PngByteQRCode(qrData);
        
        return qrCode.GetGraphic(20);
    }
}