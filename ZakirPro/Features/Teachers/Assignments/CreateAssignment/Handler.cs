using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Assignments.CreateAssignment;

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

        var tss = await _db.TeacherSubjectStages.FirstOrDefaultAsync(t => t.Id == request.TeacherSubjectStageId, cancellationToken);
        if (tss == null || tss.TeacherId != effectiveTeacherId)
            return EndpointResponse<AssignmentDto>.ForbiddenResponse("You do not own this subject/stage assignment.");

        var assignment = new Assignment
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            DueDate = request.DueDate.ToUniversalTime(),
            MaxScore = request.MaxScore > 0 ? request.MaxScore : 100m,
            TeacherSubjectStageId = request.TeacherSubjectStageId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Assignments.Add(assignment);
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

        return EndpointResponse<AssignmentDto>.SuccessResponse(dto, "Assignment created successfully.");
    }
}
