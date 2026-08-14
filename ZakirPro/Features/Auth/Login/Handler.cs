using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.Login;

public class Handler : IRequestHandler<Command, EndpointResponse<LoginResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;
    private readonly IConfiguration _configuration;

    public Handler(
        IUnitOfWork uow,
        ITokenService tokenService,
        ICurrentUserService currentUser,
        IAuditLogger auditLogger,
        IConfiguration configuration)
    {
        _uow = uow;
        _tokenService = tokenService;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
        _configuration = configuration;
    }

    public async Task<EndpointResponse<LoginResponse>> Handle(Command request, CancellationToken cancellationToken)
    {
        var userRepo = _uow.GetRepository<User>();

        // 1. Find user by email (soft-delete filter is applied — deleted users cannot log in)
        var user = await userRepo
            .Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // 2. Not found
        if (user is null)
            return EndpointResponse<LoginResponse>.ErrorResponse("Invalid credentials.");

        // 3. Account disabled
        if (!user.IsActive)
            return EndpointResponse<LoginResponse>.ErrorResponse("Account is disabled.");

        // 4. Student must be activated
        if (user is Student student && !student.IsActivated)
            return EndpointResponse<LoginResponse>.ErrorResponse("Please activate your account first.");

        // 5. Verify password
        var hasher = new PasswordHasher<User>();
        var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
            return EndpointResponse<LoginResponse>.ErrorResponse("Invalid credentials.");

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, request.Password);
        }

        // 6. Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // 7. Determine refresh token expiry from config
        var expiryDaysKey = request.RememberMe
            ? "JwtSettings:RefreshTokenRememberMeExpiryDays"
            : "JwtSettings:RefreshTokenExpiryDays";
        var expiryDays = _configuration.GetValue<int>(expiryDaysKey, request.RememberMe ? 30 : 7);

        // 8. Update user refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);
        user.UpdatedAt = DateTime.UtcNow;

        // Since Query() is AsNoTracking, calling Update() attaches + marks Modified
        userRepo.Update(user);
        await _uow.SaveChangesAsync();

        // 10. Audit log (fire-and-forget)
        _ = _auditLogger.LogAsync(
            action: "Login",
            userId: user.Id,
            targetId: user.Id,
            description: $"User {user.Email} logged in.",
            ipAddress: _currentUser.IpAddress);

        // 11. Return response
        return EndpointResponse<LoginResponse>.SuccessResponse(
            new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                User: new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString())),
            "Login successful.");
    }
}
