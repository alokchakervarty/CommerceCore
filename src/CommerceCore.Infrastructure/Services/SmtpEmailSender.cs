using System.Net;
using System.Net.Mail;
using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Infrastructure.Persistence;
using CommerceCore.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommerceCore.Infrastructure.Services;

/// <summary>
/// Sends email via the target Store's own SMTP configuration (StoreSettings),
/// so each tenant can use its own sender identity/mail provider rather than a
/// single platform-wide sender. NOTE: StoreSettings.SmtpUsernameEncrypted /
/// SmtpPasswordEncrypted are stored as plain text in this build (no encryption
/// implemented yet, despite the column name) — wire up real encryption
/// (e.g. ASP.NET Core Data Protection) before storing real credentials in production.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly AppDbContext _db;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(AppDbContext db, ILogger<SmtpEmailSender> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SendAsync(Guid storeId, string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var settings = await _db.StoreSettings.FirstOrDefaultAsync(s => s.StoreId == storeId, cancellationToken);

        if (settings == null || string.IsNullOrWhiteSpace(settings.SmtpHost))
        {
            // No SMTP configured for this store yet — fail loudly rather than silently
            // dropping the email, since a caller awaiting an OTP needs to know it wasn't sent.
            _logger.LogWarning("Email not sent to {Recipient}: Store {StoreId} has no SMTP configuration.", toAddress, storeId);
            throw new BusinessRuleException("This store has not configured an email sender yet. Contact the store administrator.");
        }

        using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort ?? 587)
        {
            EnableSsl = true,
            //Credentials = new NetworkCredential("alokchakarverty002@gmail.com","jwzwnhfnieqymfkd")
            Credentials = new NetworkCredential(settings.SmtpUsernameEncrypted, settings.SmtpPasswordEncrypted)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(settings.SenderEmail ?? settings.SmtpUsernameEncrypted ?? "no-reply@example.com", settings.SenderName ?? "Store"),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toAddress);

        await client.SendMailAsync(message, cancellationToken);
    }
}
