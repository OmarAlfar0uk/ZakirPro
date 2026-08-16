using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Assignments.DeleteAssignment;

public class Handler : IRequestHandler<Command, EndpointResponse<bool>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<bool>> Handle(Command request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<bool>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<bool>.ForbiddenResponse("Assistant not found.");
            effectiveTeacherId = assistant.TeacherId;
        }
        else
        {
            effectiveTeacherId = _currentUser.UserId.Value;
        }

        var assignment = await _db.Assignments
            .Include(a => a.TeacherSubjectStage)
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);

        if (assignment == null)
            return EndpointResponse<bool>.NotFoundResponse("Assignment not found.");

        if (assignment.TeacherSubjectStage.TeacherId != effectiveTeacherId)
            return EndpointResponse<bool>.ForbiddenResponse("You do not have permission to delete this assignment.");

        assignment.IsDeleted = true;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return EndpointResponse<bool>.SuccessResponse(true, "Assignment deleted successfully.");
    }
}
