using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Auth.ForgetPassword;

public class Handler : IRequestHandler<Command, EndpointResponse<object>>
{
    private readonly AppDbContext   _db;
    private readonly IEmailService  _emailService;

    public Handler(AppDbContext db, IEmailService emailService)
    {
        _db           = db;
        _emailService = emailService;
    }

    public async Task<EndpointResponse<object>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        // Enumeration-safe: always return the same generic message regardless of outcome.
        const string safeMessage = "If an account with that email exists, a password reset code has been sent.";

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower(), cancellationToken);

        if (user is null)
            // Do NOT reveal the user doesn't exist
            return EndpointResponse<object>.SuccessResponse(null, safeMessage);

        if (!user.IsActive)
            return EndpointResponse<object>.SuccessResponse(null, safeMessage);

        // Generate 6-digit OTP via CSPRNG (cryptographically secure)
        int otpInt  = RandomNumberGenerator.GetInt32(100_000, 1_000_000);
        string otp  = otpInt.ToString("D6");

        // Reset attempts counter on every fresh OTP request
        user.PasswordResetOtp          = otp;
        user.PasswordResetOtpExpiry    = DateTime.UtcNow.AddMinutes(15);
        user.PasswordResetOtpAttempts  = 0;
        user.PasswordResetToken        = null;
        user.PasswordResetTokenExpiry  = null;
        user.UpdatedAt                 = DateTime.UtcNow;

        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);

        // Send email — fire and forget pattern: failure doesn't surface to client
        // (the OTP has been written to DB; a retry mechanism can re-send separately)
        _ = _emailService.SendEmailAsync(
            to:       user.Email,
            subject:  "Zaker Pro — Password Reset Code",
            htmlBody: $"""
                       <h2>Password Reset Request</h2>
                       <p>Hello {user.FullName},</p>
                       <p>Your one-time password reset code is:</p>
                       <h1 style="letter-spacing:8px">{otp}</h1>
                       <p>This code expires in <strong>15 minutes</strong>.</p>
                       <p>If you did not request this, you can safely ignore this email.</p>
                       """);

        return EndpointResponse<object>.SuccessResponse(null, safeMessage);
    }
}
