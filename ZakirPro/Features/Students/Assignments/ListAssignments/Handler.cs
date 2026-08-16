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

namespace ZakirPro.Features.Students.Assignments.ListAssignments;

public class Handler : IRequestHandler<Query, EndpointResponse<List<StudentAssignmentItemDto>>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<StudentAssignmentItemDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<List<StudentAssignmentItemDto>>.ForbiddenResponse("You are not authorized to view this data.");

        // Find enrolled TeacherSubjectStages for this student
        var enrolledTssQuery = _db.StudentTeacherSubjectStages
            .Where(st => st.StudentId == request.StudentId);

        if (request.TeacherSubjectStageId.HasValue)
        {
            enrolledTssQuery = enrolledTssQuery.Where(st => st.TeacherSubjectStageId == request.TeacherSubjectStageId.Value);
        }

        var enrolledTssIds = await enrolledTssQuery
            .Select(st => st.TeacherSubjectStageId)
            .ToListAsync(cancellationToken);

        if (enrolledTssIds.Count == 0)
        {
            return EndpointResponse<List<StudentAssignmentItemDto>>.SuccessResponse([]);
        }

        // Get assignments under enrolled TSSs
        var assignments = await _db.Assignments
            .Include(a => a.TeacherSubjectStage).ThenInclude(tss => tss.Teacher)
            .Include(a => a.TeacherSubjectStage).ThenInclude(tss => tss.Subject)
            .Include(a => a.TeacherSubjectStage).ThenInclude(tss => tss.Stage)
            .Where(a => enrolledTssIds.Contains(a.TeacherSubjectStageId))
            .OrderByDescending(a => a.DueDate)
            .ToListAsync(cancellationToken);

        var assignmentIds = assignments.Select(a => a.Id).ToList();

        // Get student's submissions for these assignments
        var submissions = await _db.AssignmentSubmissions
            .Where(s => s.StudentId == request.StudentId && assignmentIds.Contains(s.AssignmentId) && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        var results = assignments.Select(a =>
        {
            var sub = submissions.FirstOrDefault(s => s.AssignmentId == a.Id);
            return new StudentAssignmentItemDto(
                AssignmentId: a.Id,
                Title: a.Title,
                Description: a.Description,
                DueDate: a.DueDate,
                MaxScore: a.MaxScore,
                TeacherSubjectStageId: a.TeacherSubjectStageId,
                TeacherName: a.TeacherSubjectStage.Teacher.FullName,
                SubjectName: a.TeacherSubjectStage.Subject.Name,
                StageName: a.TeacherSubjectStage.Stage.Name,
                SubmissionStatus: sub != null ? sub.Status.ToString() : AssignmentSubmissionStatus.NotSubmitted.ToString(),
                SubmissionId: sub?.Id,
                Score: sub?.Score,
                Feedback: sub?.Feedback,
                SubmittedAt: sub?.SubmittedAt,
                GradedAt: sub?.GradedAt
            );
        }).ToList();

        return EndpointResponse<List<StudentAssignmentItemDto>>.SuccessResponse(results);
    }
}
