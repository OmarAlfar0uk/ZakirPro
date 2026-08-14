using System.Security.Claims;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Common.Abstractions;

public interface ITokenService
{
    /// <summary>Generates a 30-minute JWT access token with userId, email, and role claims.</summary>
    string GenerateAccessToken(User user);

    /// <summary>Generates a cryptographically random refresh token.</summary>
    string GenerateRefreshToken();

    /// <summary>Validates a token (without lifetime check) and returns its claims, or null if invalid.</summary>
    ClaimsPrincipal? ValidateTokenWithoutLifetime(string token);
}
