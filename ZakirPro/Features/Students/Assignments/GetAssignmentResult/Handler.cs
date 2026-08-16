using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;
using ZakirPro.Features.Students.Assignments.SubmitAssignment;

namespace ZakirPro.Features.Students.Assignments.GetAssignmentResult;

public class Handler : IRequestHandler<Query, EndpointResponse<StudentSubmissionDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<StudentSubmissionDto>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<StudentSubmissionDto>.ForbiddenResponse("You are not authorized to view this result.");

        var assignment = await _db.Assignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);

        if (assignment == null)
            return EndpointResponse<StudentSubmissionDto>.NotFoundResponse("Assignment not found.");

        var submission = await _db.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentId == request.AssignmentId && s.StudentId == request.StudentId && !s.IsDeleted, cancellationToken);

        if (submission == null)
        {
            var notSubmittedDto = new StudentSubmissionDto(
                SubmissionId: Guid.Empty,
                AssignmentId: assignment.Id,
                StudentId: request.StudentId,
                FilePath: string.Empty,
                Status: AssignmentSubmissionStatus.NotSubmitted.ToString(),
                Score: null,
                MaxScore: assignment.MaxScore,
                Feedback: null,
                SubmittedAt: null,
                GradedAt: null
            );
            return EndpointResponse<StudentSubmissionDto>.SuccessResponse(notSubmittedDto);
        }

        var dto = new StudentSubmissionDto(
            SubmissionId: submission.Id,
            AssignmentId: submission.AssignmentId,
            StudentId: submission.StudentId,
            FilePath: submission.FilePath,
            Status: submission.Status.ToString(),
            Score: submission.Score,
            MaxScore: submission.MaxScore,
            Feedback: submission.Feedback,
            SubmittedAt: submission.SubmittedAt,
            GradedAt: submission.GradedAt
        );

        return EndpointResponse<StudentSubmissionDto>.SuccessResponse(dto);
    }
}
