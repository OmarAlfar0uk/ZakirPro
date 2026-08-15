using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.GradeEssayAnswer;

public class Handler : IRequestHandler<Command, EndpointResponse<GradeResultDto>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext       _db;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser, AppDbContext db)
    {
        _uow         = uow;
        _currentUser = currentUser;
        _db          = db;
    }

    public async Task<EndpointResponse<GradeResultDto>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<GradeResultDto>.ForbiddenResponse(
                "You are not authorized to grade exams on behalf of another teacher.");

        // Verify exam ownership
        var exam = await _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.TeacherSubjectStage)
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        if (exam is null)
            return EndpointResponse<GradeResultDto>.NotFoundResponse("Exam not found.");

        if (exam.TeacherSubjectStage.TeacherId != request.TeacherId)
            return EndpointResponse<GradeResultDto>.ForbiddenResponse("This exam does not belong to you.");

        // Load attempt with tracking
        var attempt = await _db.StudentExamAttempts
            .Include(a => a.Answers)
                .ThenInclude(sa => sa.Question)
            .FirstOrDefaultAsync(
                a => a.Id == request.AttemptId && a.ExamId == request.ExamId,
                cancellationToken);

        if (attempt is null)
            return EndpointResponse<GradeResultDto>.NotFoundResponse("Attempt not found for this exam.");

        if (attempt.AttemptStatus == AttemptStatus.InProgress)
            return EndpointResponse<GradeResultDto>.ErrorResponse(
                "Cannot grade an attempt that is still in progress.");

        if (attempt.AttemptStatus == AttemptStatus.Graded)
            return EndpointResponse<GradeResultDto>.ErrorResponse(
                "This attempt has already been fully graded.");

        // Find the specific answer
        var answer = attempt.Answers.FirstOrDefault(sa => sa.Id == request.AnswerId);
        if (answer is null)
            return EndpointResponse<GradeResultDto>.NotFoundResponse("Answer not found in this attempt.");

        if (answer.Question.Type != QuestionType.Essay)
            return EndpointResponse<GradeResultDto>.ErrorResponse(
                "Only Essay answers can be graded via this endpoint.");

        // PointsAwarded must not exceed the question's maximum
        if (request.PointsAwarded > answer.Question.Points)
            return EndpointResponse<GradeResultDto>.ErrorResponse(
                $"PointsAwarded ({request.PointsAwarded}) cannot exceed the question's maximum points ({answer.Question.Points}).");

        // Grade this answer — IsCorrect stays null for Essay (per design §3.3 amendment)
        answer.PointsAwarded = request.PointsAwarded;
        answer.UpdatedAt     = DateTime.UtcNow;

        // Optionally update the per-attempt teacher feedback
        if (request.TeacherFeedback is not null)
            attempt.TeacherFeedback = request.TeacherFeedback;

        // Check if all Essay answers are now graded
        var allEssayAnswers = attempt.Answers
            .Where(sa => sa.Question.Type == QuestionType.Essay)
            .ToList();

        int pendingCount = allEssayAnswers.Count(sa => sa.PointsAwarded is null);

        if (pendingCount == 0)
        {
            // All essays graded — finalise the attempt
            int essayScore          = allEssayAnswers.Sum(sa => sa.PointsAwarded ?? 0);
            attempt.EssayScore      = essayScore;
            attempt.FinalScore      = (attempt.AutoGradedScore ?? 0) + essayScore;
            attempt.IsPassed        = attempt.MaxScore > 0 &&
                                      ((decimal)attempt.FinalScore.Value / attempt.MaxScore * 100m) >= exam.PassThresholdPercent;
            attempt.AttemptStatus   = AttemptStatus.Graded;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return EndpointResponse<GradeResultDto>.SuccessResponse(
            new GradeResultDto(
                attempt.Id,
                attempt.AttemptStatus.ToString(),
                pendingCount,
                attempt.FinalScore,
                attempt.IsPassed),
            pendingCount == 0
                ? "Essay graded. Attempt is now fully graded."
                : $"Essay graded. {pendingCount} essay answer(s) still pending.");
    }
}
