using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Wishapp.Web.Infrastructure.Email;

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        using var client = new SmtpClient(_options.Host, _options.Port);
        client.EnableSsl = true;
        client.Timeout = 10_000;

        if (_options.Username is not null && _options.Password is not null)
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);

        using var message = new MailMessage(_options.From, to, subject, body);
        message.IsBodyHtml = true;

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

        await client.SendMailAsync(message, timeoutCts.Token);
    }
}
