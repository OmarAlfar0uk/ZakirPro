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

namespace ZakirPro.Features.Students.Assignments.SubmitAssignment;

public class Handler : IRequestHandler<Command, EndpointResponse<StudentSubmissionDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileService _fileService;

    public Handler(AppDbContext db, ICurrentUserService currentUser, IFileService fileService)
    {
        _db = db;
        _currentUser = currentUser;
        _fileService = fileService;
    }

    public async Task<EndpointResponse<StudentSubmissionDto>> Handle(Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<StudentSubmissionDto>.ForbiddenResponse("You are not authorized to submit for this student.");

        var assignment = await _db.Assignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);

        if (assignment == null)
            return EndpointResponse<StudentSubmissionDto>.NotFoundResponse("Assignment not found.");

        // Verify student is enrolled in the assignment's TeacherSubjectStage
        var isEnrolled = await _db.StudentTeacherSubjectStages
            .AnyAsync(st => st.StudentId == request.StudentId && st.TeacherSubjectStageId == assignment.TeacherSubjectStageId, cancellationToken);

        if (!isEnrolled)
            return EndpointResponse<StudentSubmissionDto>.ForbiddenResponse("You are not enrolled in the course for this assignment.");

        // Validate file extension and size before touching disk
        if (!_fileService.IsValidAssignmentExtension(request.File))
            return EndpointResponse<StudentSubmissionDto>.ErrorResponse("File extension is not allowed. Allowed types: .pdf, .doc, .docx, .txt, .pptx, .xlsx, .jpg, .jpeg, .png");

        if (!_fileService.IsValidAssignmentFileSize(request.File))
            return EndpointResponse<StudentSubmissionDto>.ErrorResponse("File size exceeds the maximum allowed limit of 20MB or is empty.");

        // Save file
        var fileUrl = await _fileService.SaveAssignmentFileAsync(request.File);

        var now = DateTime.UtcNow;
        var submissionStatus = now > assignment.DueDate
            ? AssignmentSubmissionStatus.Late
            : AssignmentSubmissionStatus.Submitted;

        // Upsert submission
        var existing = await _db.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentId == request.AssignmentId && s.StudentId == request.StudentId, cancellationToken);

        AssignmentSubmission submission;
        if (existing != null)
        {
            // Delete old file
            _fileService.DeleteFile(existing.FilePath);

            existing.FilePath = fileUrl;
            existing.Status = submissionStatus;
            existing.SubmittedAt = now;
            existing.Score = null;
            existing.Feedback = null;
            existing.GradedAt = null;
            existing.UpdatedAt = now;

            submission = existing;
        }
        else
        {
            submission = new AssignmentSubmission
            {
                AssignmentId = request.AssignmentId,
                StudentId = request.StudentId,
                FilePath = fileUrl,
                Status = submissionStatus,
                MaxScore = assignment.MaxScore,
                SubmittedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.AssignmentSubmissions.Add(submission);
        }

        await _db.SaveChangesAsync(cancellationToken);

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

        return EndpointResponse<StudentSubmissionDto>.SuccessResponse(dto, "Assignment submitted successfully.");
    }
}
