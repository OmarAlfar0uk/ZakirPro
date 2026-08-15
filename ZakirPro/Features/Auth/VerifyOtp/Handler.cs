using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Models;
using ZakirPro.Data;

namespace ZakirPro.Features.Auth.VerifyOtp;

public class Handler : IRequestHandler<Command, EndpointResponse<VerifyOtpResponse>>
{
    private const int MaxAttempts = 5;

    private readonly AppDbContext _db;

    public Handler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EndpointResponse<VerifyOtpResponse>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower(), cancellationToken);

        // Uniform error message — don't reveal whether email, OTP, or rate-limit is the problem
        const string invalidMsg = "Invalid or expired code.";

        if (user is null || !user.IsActive)
            return EndpointResponse<VerifyOtpResponse>.ErrorResponse(invalidMsg);

        // No OTP on record
        if (string.IsNullOrEmpty(user.PasswordResetOtp))
            return EndpointResponse<VerifyOtpResponse>.ErrorResponse(invalidMsg);

        // Rate-limit: too many failed attempts — OTP is effectively burned
        if (user.PasswordResetOtpAttempts >= MaxAttempts)
        {
            // Clear the OTP so the user must request a new one
            user.PasswordResetOtp         = null;
            user.PasswordResetOtpExpiry   = null;
            user.PasswordResetOtpAttempts = 0;
            user.UpdatedAt                = DateTime.UtcNow;
            _db.Users.Update(user);
            await _db.SaveChangesAsync(cancellationToken);
            return EndpointResponse<VerifyOtpResponse>.ErrorResponse(
                "Too many failed attempts. Please request a new code.");
        }

        // OTP expired
        if (user.PasswordResetOtpExpiry is null || user.PasswordResetOtpExpiry < DateTime.UtcNow)
        {
            user.PasswordResetOtp         = null;
            user.PasswordResetOtpExpiry   = null;
            user.PasswordResetOtpAttempts = 0;
            user.UpdatedAt                = DateTime.UtcNow;
            _db.Users.Update(user);
            await _db.SaveChangesAsync(cancellationToken);
            return EndpointResponse<VerifyOtpResponse>.ErrorResponse(invalidMsg);
        }

        // OTP mismatch — increment attempt counter
        if (user.PasswordResetOtp != request.Otp)
        {
            user.PasswordResetOtpAttempts++;
            user.UpdatedAt = DateTime.UtcNow;
            _db.Users.Update(user);
            await _db.SaveChangesAsync(cancellationToken);
            int remaining = MaxAttempts - user.PasswordResetOtpAttempts;
            return EndpointResponse<VerifyOtpResponse>.ErrorResponse(
                remaining > 0
                    ? $"Invalid code. {remaining} attempt(s) remaining."
                    : "Invalid code. No attempts remaining — please request a new code.");
        }

        // OTP matched — generate a short-lived reset token
        var resetToken  = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenExpiry = DateTime.UtcNow.AddMinutes(15);

        user.PasswordResetOtp          = null;        // Consume the OTP
        user.PasswordResetOtpExpiry    = null;
        user.PasswordResetOtpAttempts  = 0;
        user.PasswordResetToken        = resetToken;
        user.PasswordResetTokenExpiry  = tokenExpiry;
        user.UpdatedAt                 = DateTime.UtcNow;

        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);

        return EndpointResponse<VerifyOtpResponse>.SuccessResponse(
            new VerifyOtpResponse(resetToken, tokenExpiry),
            "Code verified. Use the reset token to set a new password within 15 minutes.");
    }
}
