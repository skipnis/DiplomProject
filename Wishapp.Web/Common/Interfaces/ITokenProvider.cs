using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Common.Interfaces;

public interface ITokenProvider
{
    string Create(User user);
}
