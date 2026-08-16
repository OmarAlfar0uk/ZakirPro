using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;
using ZakirPro.Features.Teachers.Assignments.CreateAssignment;

namespace ZakirPro.Features.Teachers.Assignments.UpdateAssignment;

public class Handler : IRequestHandler<Command, EndpointResponse<AssignmentDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<AssignmentDto>> Handle(Command request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<AssignmentDto>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<AssignmentDto>.ForbiddenResponse("Assistant not found.");
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
            return EndpointResponse<AssignmentDto>.NotFoundResponse("Assignment not found.");

        if (assignment.TeacherSubjectStage.TeacherId != effectiveTeacherId)
            return EndpointResponse<AssignmentDto>.ForbiddenResponse("You do not have permission to update this assignment.");

        assignment.Title = request.Title.Trim();
        assignment.Description = request.Description?.Trim();
        assignment.DueDate = request.DueDate.ToUniversalTime();
        if (request.MaxScore.HasValue && request.MaxScore.Value > 0)
        {
            assignment.MaxScore = request.MaxScore.Value;
        }
        assignment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new AssignmentDto(
            assignment.Id,
            assignment.Title,
            assignment.Description,
            assignment.DueDate,
            assignment.MaxScore,
            assignment.TeacherSubjectStageId,
            assignment.CreatedAt
        );

        return EndpointResponse<AssignmentDto>.SuccessResponse(dto, "Assignment updated successfully.");
    }
}
