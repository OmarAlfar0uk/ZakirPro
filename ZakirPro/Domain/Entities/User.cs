using ZakirPro.Domain.Enums;

namespace ZakirPro.Domain.Entities;

/// <summary>
/// Abstract base for all user types — Table-Per-Hierarchy (TPH) via EF Core discriminator.
/// </summary>
public abstract class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Refresh token stored on the user for simplicity (single active session).
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    /// <summary>UTC timestamp of the user's most recent successful login. Null until first login.</summary>
    public DateTime? LastLoginAt { get; set; }

    // ── Password reset flow ───────────────────────────────────────────────────
    /// <summary>6-digit OTP sent to the user's email for identity verification.</summary>
    public string? PasswordResetOtp { get; set; }
    /// <summary>UTC expiry of the OTP (15 minutes from generation).</summary>
    public DateTime? PasswordResetOtpExpiry { get; set; }
    /// <summary>Number of failed OTP verification attempts. Reset to 0 on successful verify or re-request.</summary>
    public int PasswordResetOtpAttempts { get; set; } = 0;
    /// <summary>Cryptographically random token returned after OTP verification; used to authorise the final reset.</summary>
    public string? PasswordResetToken { get; set; }
    /// <summary>UTC expiry of the reset token (15 minutes from successful OTP verification).</summary>
    public DateTime? PasswordResetTokenExpiry { get; set; }
}
