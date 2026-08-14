using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Common.Infrastructure;

public class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IConfiguration config, ILogger<MailKitEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendActivationCodeAsync(string to, string fullName, string code)
    {
        var html = $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Arial,sans-serif;background:#f4f4f4;padding:30px;">
              <div style="max-width:480px;margin:0 auto;background:#fff;border-radius:8px;padding:32px;">
                <h2 style="color:#1e40af;margin-bottom:8px;">Welcome to Zaker Pro, {System.Net.WebUtility.HtmlEncode(fullName)}!</h2>
                <p style="color:#374151;">Your account has been created. Use the code below to activate your account and set your password.</p>
                <div style="background:#eff6ff;border-radius:8px;padding:24px;text-align:center;margin:24px 0;">
                  <span style="font-size:2rem;font-weight:700;letter-spacing:8px;color:#1e40af;">{code}</span>
                </div>
                <p style="color:#6b7280;font-size:0.85rem;">This code expires in <strong>24 hours</strong>. If you did not request this, please ignore this email.</p>
              </div>
            </body>
            </html>
            """;

        await SendEmailAsync(to, "Activate Your Zaker Pro Account", html);
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["EmailSettings:FromName"] ?? "Zaker Pro",
            _config["EmailSettings:FromEmail"] ?? "noreply@zakirpro.com"));
        message.To.Add(new MailboxAddress(string.Empty, to));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _config["EmailSettings:Host"]!,
            int.Parse(_config["EmailSettings:Port"] ?? "587"),
            SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(
            _config["EmailSettings:Username"]!,
            _config["EmailSettings:Password"]!);
        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);
    }
}
