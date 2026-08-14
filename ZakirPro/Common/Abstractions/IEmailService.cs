namespace ZakirPro.Common.Abstractions;

public interface IEmailService
{
    Task SendActivationCodeAsync(string to, string fullName, string code);
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
