using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Email;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Users.Features.SendOtp;

public sealed class SendOtpHandler(
    ApplicationDbContext db,
    IEmailSender emailSender)
    : ICommandHandler<SendOtpCommand>
{
    private static readonly Error TooManyRequests =
        Error.Failure("Otp.TooManyRequests", "Please wait before requesting a new code.");

    public async Task<Result> HandleAsync(SendOtpCommand command, CancellationToken ct = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();

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
            "Ваш код входа в Wishapp",
            $"<p>Код для входа: <strong>{code}</strong></p><p>Действителен 10 минут.</p>",
            ct);

        return Result.Success();
    }
}
