using ZakirPro.Domain.Enums;

namespace ZakirPro.Common.Abstractions;

/// <summary>Extracts the current authenticated user's identity from the HTTP context.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
}
