using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.DeleteUser;

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
        // 1. Get caller identity
        var callerRole = _currentUser.Role;
        var callerId = _currentUser.UserId;

        if (callerRole is null || callerId is null)
            return EndpointResponse<string>.ErrorResponse("Unauthenticated.");

        // 2. Find target user (ignore all query filters)
        var repo = _uow.GetRepository<User>();
        var user = await repo.QueryIgnoreFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        // 3. Target not found
        if (user is null)
            return EndpointResponse<string>.NotFoundResponse("User not found.");

        // 4. Rule: cannot delete SuperAdmin accounts
        if (user.Role == UserRole.SuperAdmin)
            return EndpointResponse<string>.ForbiddenResponse("SuperAdmin accounts cannot be deleted.");

        // 4b. Rule: Admin cannot delete other Admin accounts
        if (callerRole == UserRole.Admin && user.Role == UserRole.Admin)
            return EndpointResponse<string>.ForbiddenResponse("Admins cannot delete other Admin accounts.");

        // 4c. Rule: cannot self-delete
        if (user.Id == callerId.Value)
            return EndpointResponse<string>.ForbiddenResponse("You cannot delete your own account.");

        string auditAction;

        if (callerRole == UserRole.Admin)
        {
            // 5. Admin: soft delete
            repo.SoftDelete(user);
            auditAction = "SoftDeleteUser";
        }
        else
        {
            // 6. SuperAdmin: hard delete
            repo.HardDelete(user);
            auditAction = "HardDeleteUser";
        }

        await _uow.SaveChangesAsync();

        // 7. Audit log (fire-and-forget)
        _ = _auditLogger.LogAsync(
            action: auditAction,
            userId: callerId.Value,
            targetId: user.Id,
            description: $"User '{user.Email}' was {(callerRole == UserRole.Admin ? "soft" : "hard")} deleted by {_currentUser.Email}.",
            ipAddress: _currentUser.IpAddress);

        // 8. Return success
        return EndpointResponse<string>.SuccessResponse(
            $"User '{user.FullName}' deleted successfully.",
            $"User '{user.FullName}' deleted successfully.");
    }
}
