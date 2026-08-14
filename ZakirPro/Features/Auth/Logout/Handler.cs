using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Auth.Logout;

public class Handler : IRequestHandler<Command, EndpointResponse<string>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser, IAuditLogger auditLogger)
    {
        _uow = uow;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
    }

    public async Task<EndpointResponse<string>> Handle(Command request, CancellationToken cancellationToken)
    {
        // 1. Get UserId from ICurrentUserService
        var userId = _currentUser.UserId;
        if (userId is null)
            return EndpointResponse<string>.ErrorResponse("Unauthenticated.");

        // 2. Find user by ID
        var repo = _uow.GetRepository<User>();
        var user = await repo.Query()
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null)
            return EndpointResponse<string>.NotFoundResponse("User not found.");

        // 3. Revoke refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        // 4. Persist
        repo.Update(user);
        await _uow.SaveChangesAsync();

        // 5. Audit log (fire-and-forget)
        _ = _auditLogger.LogAsync(
            action: "Logout",
            userId: userId.Value,
            targetId: userId.Value,
            description: $"User '{user.Email}' logged out.",
            ipAddress: _currentUser.IpAddress);

        // 6. Return success
        return EndpointResponse<string>.SuccessResponse("Logged out successfully.", "Logged out successfully.");
    }
}
