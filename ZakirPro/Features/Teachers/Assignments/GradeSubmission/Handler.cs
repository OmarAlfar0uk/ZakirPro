using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Assignments.GradeSubmission;

public class Handler : IRequestHandler<Command, EndpointResponse<GradedSubmissionDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<GradedSubmissionDto>> Handle(Command request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<GradedSubmissionDto>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<GradedSubmissionDto>.ForbiddenResponse("Assistant not found.");
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
            return EndpointResponse<GradedSubmissionDto>.NotFoundResponse("Assignment not found.");

        if (assignment.TeacherSubjectStage.TeacherId != effectiveTeacherId)
            return EndpointResponse<GradedSubmissionDto>.ForbiddenResponse("You do not have permission to grade submissions for this assignment.");

        var submission = await _db.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId && s.AssignmentId == request.AssignmentId, cancellationToken);

        if (submission == null)
            return EndpointResponse<GradedSubmissionDto>.NotFoundResponse("Submission not found for this assignment.");

        if (request.Score > submission.MaxScore)
            return EndpointResponse<GradedSubmissionDto>.ErrorResponse($"Score ({request.Score}) cannot exceed max score ({submission.MaxScore}).");

        submission.Score = request.Score;
        submission.Feedback = request.Feedback?.Trim();
        submission.Status = AssignmentSubmissionStatus.Graded;
        submission.GradedAt = DateTime.UtcNow;
        submission.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new GradedSubmissionDto(
            SubmissionId: submission.Id,
            AssignmentId: submission.AssignmentId,
            StudentId: submission.StudentId,
            Score: submission.Score.Value,
            MaxScore: submission.MaxScore,
            Feedback: submission.Feedback,
            Status: submission.Status.ToString(),
            GradedAt: submission.GradedAt
        );

        return EndpointResponse<GradedSubmissionDto>.SuccessResponse(dto, "Submission graded successfully.");
    }
}
