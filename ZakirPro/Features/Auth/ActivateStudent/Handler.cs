using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Auth.ActivateStudent;

public class Handler : IRequestHandler<Command, EndpointResponse<string>>
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditLogger _auditLogger;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, IAuditLogger auditLogger, ICurrentUserService currentUser)
    {
        _uow = uow;
        _auditLogger = auditLogger;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<string>> Handle(Command request, CancellationToken cancellationToken)
    {
        var studentRepo = _uow.GetRepository<Student>();

        // 1. Find student by activation code (bypass soft-delete filter for unactivated accounts)
        var student = await studentRepo
            .QueryIgnoreFilters()
            .FirstOrDefaultAsync(
                s => s.ActivationCode == request.ActivationCode && !s.IsActivated,
                cancellationToken);

        if (student is null)
            return EndpointResponse<string>.NotFoundResponse("Invalid activation code.");

        // 3. Check expiry
        if (student.ActivationCodeExpiry.HasValue && student.ActivationCodeExpiry.Value < DateTime.UtcNow)
            return EndpointResponse<string>.ErrorResponse("Activation code has expired.");

        // Get the tracked entity so EF can update it
        var trackedStudent = await studentRepo.GetByIdIgnoreFiltersAsync(student.Id);
        if (trackedStudent is null)
            return EndpointResponse<string>.NotFoundResponse("Student not found.");

        // 4. Hash new password
        var hasher = new PasswordHasher<User>();
        trackedStudent.PasswordHash = hasher.HashPassword(trackedStudent, request.NewPassword);

        // 5. Activate
        trackedStudent.IsActivated = true;
        trackedStudent.ActivationCode = null;
        trackedStudent.ActivationCodeExpiry = null;
        trackedStudent.UpdatedAt = DateTime.UtcNow;

        studentRepo.Update(trackedStudent);
        await _uow.SaveChangesAsync();

        // Audit log (fire-and-forget)
        _ = _auditLogger.LogAsync(
            action: "ActivateStudent",
            userId: trackedStudent.Id,
            targetId: trackedStudent.Id,
            description: $"Student account activated for {trackedStudent.Email}.",
            ipAddress: _currentUser.IpAddress);

        return EndpointResponse<string>.SuccessResponse(
            "Account activated successfully. You can now log in.",
            "Account activated successfully. You can now log in.");
    }
}
