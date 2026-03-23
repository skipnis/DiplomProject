using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Login;

public sealed class AdminLoginHandler(ApplicationDbContext db, ITokenProvider tokenProvider)
    : ICommandHandler<AdminLoginCommand, AdminLoginResponse>
{
    public async Task<Result<AdminLoginResponse>> HandleAsync(
        AdminLoginCommand command,
        CancellationToken ct = default)
    {
        var admin = await db.AdminUsers
            .FirstOrDefaultAsync(a => a.Username == command.Username, ct);

        if (admin is null || !BCrypt.Net.BCrypt.Verify(command.Password, admin.PasswordHash))
        {
            return Error.Unauthorized("Admin.InvalidCredentials", "Invalid username or password");
        }

        var token = tokenProvider.CreateForAdmin(admin);

        return new AdminLoginResponse(token);
    }
}
