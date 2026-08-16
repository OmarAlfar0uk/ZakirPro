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

namespace ZakirPro.Features.Teachers.Assignments.GetAssignmentSubmissions;

public class Handler : IRequestHandler<Query, EndpointResponse<AssignmentSubmissionsOverviewDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<AssignmentSubmissionsOverviewDto>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<AssignmentSubmissionsOverviewDto>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<AssignmentSubmissionsOverviewDto>.ForbiddenResponse("Assistant not found.");
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
            return EndpointResponse<AssignmentSubmissionsOverviewDto>.NotFoundResponse("Assignment not found.");

        if (assignment.TeacherSubjectStage.TeacherId != effectiveTeacherId)
            return EndpointResponse<AssignmentSubmissionsOverviewDto>.ForbiddenResponse("You do not have permission to view submissions for this assignment.");

        // Get all enrolled active students in this TSS
        var enrolledStudents = await _db.StudentTeacherSubjectStages
            .Include(st => st.Student)
            .Where(st => st.TeacherSubjectStageId == assignment.TeacherSubjectStageId && !st.Student.IsDeleted)
            .Select(st => st.Student)
            .ToListAsync(cancellationToken);

        // Get all non-deleted submissions for this assignment
        var submissions = await _db.AssignmentSubmissions
            .Where(s => s.AssignmentId == request.AssignmentId && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        var roster = enrolledStudents.Select(student =>
        {
            var sub = submissions.FirstOrDefault(s => s.StudentId == student.Id);
            return new StudentSubmissionRosterDto(
                StudentId: student.Id,
                StudentFullName: student.FullName,
                StudentEmail: student.Email,
                SubmissionId: sub?.Id,
                Status: sub != null ? sub.Status.ToString() : AssignmentSubmissionStatus.NotSubmitted.ToString(),
                FilePath: sub?.FilePath,
                Score: sub?.Score,
                MaxScore: sub?.MaxScore ?? assignment.MaxScore,
                Feedback: sub?.Feedback,
                SubmittedAt: sub?.SubmittedAt,
                GradedAt: sub?.GradedAt
            );
        }).OrderBy(r => r.StudentFullName).ToList();

        var submittedCount = submissions.Count(s => s.Status != AssignmentSubmissionStatus.NotSubmitted);
        var gradedCount = submissions.Count(s => s.Status == AssignmentSubmissionStatus.Graded);

        var overview = new AssignmentSubmissionsOverviewDto(
            AssignmentId: assignment.Id,
            Title: assignment.Title,
            DueDate: assignment.DueDate,
            MaxScore: assignment.MaxScore,
            TotalEnrolled: enrolledStudents.Count,
            SubmittedCount: submittedCount,
            GradedCount: gradedCount,
            Submissions: roster
        );

        return EndpointResponse<AssignmentSubmissionsOverviewDto>.SuccessResponse(overview);
    }
}
