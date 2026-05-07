using System.Security.Cryptography;
using System.Text;

namespace Wishapp.Web.Users;

internal static class OtpGenerator
{
    public static (string Code, string Hash) Generate()
    {
        var number = RandomNumberGenerator.GetInt32(0, 1_000_000);
        var code = number.ToString("D6");
        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
        return (code, hash);
    }
}
