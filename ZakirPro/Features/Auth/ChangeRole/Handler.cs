using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.ChangeRole;

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
        // 1. Find user by ID
        var repo = _uow.GetRepository<User>();
        var user = await repo.Query()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return EndpointResponse<string>.NotFoundResponse("User not found.");

        // 2. Cannot change SuperAdmin's role
        if (user.Role == UserRole.SuperAdmin)
            return EndpointResponse<string>.ForbiddenResponse("Cannot change the role of a SuperAdmin account.");

        var previousRole = user.Role;

        // 3. Set new role
        user.Role = request.NewRole;

        // 4. Persist
        repo.Update(user);
        await _uow.SaveChangesAsync();

        // 5. Audit log (fire-and-forget)
        _ = _auditLogger.LogAsync(
            action: "ChangeRole",
            userId: _currentUser.UserId,
            targetId: user.Id,
            description: $"User '{user.Email}' role changed from '{previousRole}' to '{request.NewRole}' by '{_currentUser.Email}'.",
            ipAddress: _currentUser.IpAddress);

        // 6. Return success
        return EndpointResponse<string>.SuccessResponse(
            $"User '{user.FullName}' role changed to '{request.NewRole}' successfully.",
            $"Role updated successfully.");
    }
}
