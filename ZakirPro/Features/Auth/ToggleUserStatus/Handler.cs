using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.ToggleUserStatus;

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
        // 1. Find user by ID (ignore soft-delete filter, check manually)
        var repo = _uow.GetRepository<User>();
        var user = await repo.QueryIgnoreFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user is null)
            return EndpointResponse<string>.NotFoundResponse("User not found.");

        // 2. Cannot toggle a SuperAdmin account
        if (user.Role == UserRole.SuperAdmin)
            return EndpointResponse<string>.ForbiddenResponse("Cannot modify a SuperAdmin account.");

        // 3. Toggle IsActive
        user.IsActive = !user.IsActive;

        // 4. Persist
        repo.Update(user);
        await _uow.SaveChangesAsync();

        // 5. Audit log (fire-and-forget)
        var action = user.IsActive ? "UserEnabled" : "UserDisabled";
        var description = user.IsActive
            ? $"User '{user.Email}' has been enabled."
            : $"User '{user.Email}' has been disabled.";

        _ = _auditLogger.LogAsync(
            action: action,
            userId: _currentUser.UserId,
            targetId: user.Id,
            description: description,
            ipAddress: _currentUser.IpAddress);

        // 6. Return success with new status message
        var statusMessage = user.IsActive
            ? $"User '{user.FullName}' has been enabled successfully."
            : $"User '{user.FullName}' has been disabled successfully.";

        return EndpointResponse<string>.SuccessResponse(statusMessage, statusMessage);
    }
}
