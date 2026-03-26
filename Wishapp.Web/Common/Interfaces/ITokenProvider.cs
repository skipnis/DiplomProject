using Wishapp.Web.Admin.Entities;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Common.Interfaces;

public interface ITokenProvider
{
    string Create(User user);
    string CreateForAdmin(AdminUser admin);
    string CreateRefreshToken();
    string HashToken(string token);
}
