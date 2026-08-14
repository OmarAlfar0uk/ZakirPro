using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Auth.Refresh;

public class Handler : IRequestHandler<Command, EndpointResponse<RefreshResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public Handler(IUnitOfWork uow, ITokenService tokenService, IConfiguration configuration)
    {
        _uow = uow;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<EndpointResponse<RefreshResponse>> Handle(Command request, CancellationToken cancellationToken)
    {
        // 1. Validate the access token without lifetime check
        var principal = _tokenService.ValidateTokenWithoutLifetime(request.AccessToken);
        if (principal is null)
            return EndpointResponse<RefreshResponse>.ErrorResponse("Invalid access token.");

        // 2. Extract user ID from claims
        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return EndpointResponse<RefreshResponse>.ErrorResponse("Invalid token claims.");

        // 3. Find user by ID (ignore soft-delete filter, but check manually)
        var repo = _uow.GetRepository<User>();
        var user = await repo.QueryIgnoreFilters()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        // 4. Validate user and refresh token
        if (user is null)
            return EndpointResponse<RefreshResponse>.NotFoundResponse("User not found.");

        if (!user.IsActive)
            return EndpointResponse<RefreshResponse>.ErrorResponse("Account is disabled.");

        if (user.RefreshToken != request.RefreshToken)
            return EndpointResponse<RefreshResponse>.ErrorResponse("Invalid refresh token.");

        if (user.RefreshTokenExpiry is null || user.RefreshTokenExpiry <= DateTime.UtcNow)
            return EndpointResponse<RefreshResponse>.ErrorResponse("Refresh token has expired. Please log in again.");

        // 5. Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // 6. Infer RememberMe from the remaining lifetime of the current refresh token
        var isRememberMe = (user.RefreshTokenExpiry.Value - DateTime.UtcNow).TotalDays > 7;
        var expiryDaysKey = isRememberMe
            ? "JwtSettings:RefreshTokenRememberMeExpiryDays"
            : "JwtSettings:RefreshTokenExpiryDays";
        var expiryDays = _configuration.GetValue<int>(expiryDaysKey, isRememberMe ? 30 : 7);

        // 7. Update user's refresh token
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);
        user.UpdatedAt = DateTime.UtcNow;
        repo.Update(user);
        await _uow.SaveChangesAsync();

        // 8. Return new tokens
        return EndpointResponse<RefreshResponse>.SuccessResponse(
            new RefreshResponse(newAccessToken, newRefreshToken),
            "Tokens refreshed successfully.");
    }
}
