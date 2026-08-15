using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Auth.ResetPassword;

public class Handler : IRequestHandler<Command, EndpointResponse<object>>
{
    private readonly AppDbContext _db;

    public Handler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EndpointResponse<object>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        const string invalidMsg = "Invalid or expired reset token.";

        var user = await _db.Users
            .FirstOrDefaultAsync(
                u => u.PasswordResetToken == request.ResetToken,
                cancellationToken);

        if (user is null || !user.IsActive)
            return EndpointResponse<object>.ErrorResponse(invalidMsg);

        if (user.PasswordResetTokenExpiry is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return EndpointResponse<object>.ErrorResponse(invalidMsg);

        // Rehash the new password
        var hasher          = new PasswordHasher<User>();
        user.PasswordHash   = hasher.HashPassword(user, request.NewPassword);

        // Clear ALL reset state
        user.PasswordResetToken        = null;
        user.PasswordResetTokenExpiry  = null;
        user.PasswordResetOtp          = null;
        user.PasswordResetOtpExpiry    = null;
        user.PasswordResetOtpAttempts  = 0;

        // Force logout on all sessions by clearing refresh token
        user.RefreshToken        = null;
        user.RefreshTokenExpiry  = null;

        user.UpdatedAt = DateTime.UtcNow;

        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);

        return EndpointResponse<object>.SuccessResponse(
            null, "Password reset successful. Please log in with your new password.");
    }
}
