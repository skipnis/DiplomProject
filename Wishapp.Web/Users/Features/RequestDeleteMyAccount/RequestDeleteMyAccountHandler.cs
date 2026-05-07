using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Email;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Users.Features.RequestDeleteMyAccount;

public sealed class RequestDeleteMyAccountHandler(
    ApplicationDbContext db,
    IEmailSender emailSender)
    : ICommandHandler<RequestDeleteMyAccountCommand>
{
    private static readonly Error TooManyRequests =
        Error.Failure("Otp.TooManyRequests", "Please wait before requesting a new code.");

    public async Task<Result> HandleAsync(RequestDeleteMyAccountCommand command, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

        if (user is null)
            return Error.NotFound("Users.NotFound", "User not found");

        var email = user.Email.Trim().ToLowerInvariant();

        var recentOtp = await db.EmailOtps
            .Where(o => o.Email == email && !o.UsedAt.HasValue)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (recentOtp is not null && DateTime.UtcNow - recentOtp.CreatedAt < TimeSpan.FromSeconds(60))
            return TooManyRequests;

        var (code, codeHash) = OtpGenerator.Generate();

        db.EmailOtps.Add(EmailOtp.Create(email, codeHash));
        await db.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            email,
            "Подтверждение удаления аккаунта Wishapp",
            $"<p>Код для удаления аккаунта: <strong>{code}</strong></p><p>Действителен 10 минут.</p><p>Если вы не запрашивали удаление — просто проигнорируйте это письмо.</p>",
            ct);

        return Result.Success();
    }
}
