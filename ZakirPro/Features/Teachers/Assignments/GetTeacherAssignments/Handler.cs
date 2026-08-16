using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Assignments.GetTeacherAssignments;

public class Handler : IRequestHandler<Query, EndpointResponse<List<TeacherAssignmentItemDto>>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<TeacherAssignmentItemDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<List<TeacherAssignmentItemDto>>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<List<TeacherAssignmentItemDto>>.ForbiddenResponse("Assistant not found.");
            effectiveTeacherId = assistant.TeacherId;
        }
        else
        {
            effectiveTeacherId = _currentUser.UserId.Value;
        }

        var query = _db.Assignments
            .Include(a => a.TeacherSubjectStage).ThenInclude(tss => tss.Subject)
            .Include(a => a.TeacherSubjectStage).ThenInclude(tss => tss.Stage)
            .Include(a => a.Submissions)
            .Where(a => a.TeacherSubjectStage.TeacherId == effectiveTeacherId);

        if (request.TeacherSubjectStageId.HasValue)
        {
            query = query.Where(a => a.TeacherSubjectStageId == request.TeacherSubjectStageId.Value);
        }

        var assignments = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new TeacherAssignmentItemDto(
                a.Id,
                a.Title,
                a.Description,
                a.DueDate,
                a.MaxScore,
                a.TeacherSubjectStageId,
                a.TeacherSubjectStage.Subject.Name,
                a.TeacherSubjectStage.Stage.Name,
                a.Submissions.Count(s => !s.IsDeleted),
                a.Submissions.Count(s => !s.IsDeleted && s.Status == AssignmentSubmissionStatus.Graded),
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return EndpointResponse<List<TeacherAssignmentItemDto>>.SuccessResponse(assignments);
    }
}
