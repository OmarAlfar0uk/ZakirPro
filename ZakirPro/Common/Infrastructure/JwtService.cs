using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ZakirPro.Common.Abstractions;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Common.Infrastructure;

public class JwtService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;

    // Cache key pattern: "roles_{userId}"
    private static string RoleCacheKey(Guid userId) => $"roles_{userId}";
    private static readonly TimeSpan RoleCacheExpiry = TimeSpan.FromMinutes(15);

    public JwtService(IConfiguration config, IMemoryCache cache)
    {
        _config = config;
        _cache = cache;
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,           user.Email),
            new Claim(ClaimTypes.Role,            user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Cache role for 15 min to avoid repeated DB look-ups.
        _cache.Set(RoleCacheKey(user.Id), user.Role.ToString(), RoleCacheExpiry);

        var expiryMinutes = int.TryParse(_config["JwtSettings:AccessTokenExpiryMinutes"], out var m) ? m : 30;

        var token = new JwtSecurityToken(
            issuer:             _config["JwtSettings:Issuer"],
            audience:           _config["JwtSettings:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? ValidateTokenWithoutLifetime(string token)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = _config["JwtSettings:Issuer"],
            ValidateAudience         = true,
            ValidAudience            = _config["JwtSettings:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = key,
            ValidateLifetime         = false   // intentionally skipped for refresh flow
        };

        try
        {
            var handler    = new JwtSecurityTokenHandler();
            var principal  = handler.ValidateToken(token, validationParams, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
