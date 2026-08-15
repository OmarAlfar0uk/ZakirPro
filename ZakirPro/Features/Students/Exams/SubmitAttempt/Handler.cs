using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Students.Exams.SubmitAttempt;

public class Handler : IRequestHandler<Command, EndpointResponse<SubmissionResultDto>>
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

    public async Task<EndpointResponse<SubmissionResultDto>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<SubmissionResultDto>.ForbiddenResponse(
                "You are not authorized to submit on behalf of another student.");

        // Load attempt with tracking
        var attempt = await _db.StudentExamAttempts
            .Include(a => a.Exam)
            .Include(a => a.Answers)
                .ThenInclude(sa => sa.Question)
            .Include(a => a.Answers)
                .ThenInclude(sa => sa.SelectedChoice)
            .FirstOrDefaultAsync(a =>
                a.Id == request.AttemptId &&
                a.StudentId == request.StudentId,
                cancellationToken);

        if (attempt is null)
            return EndpointResponse<SubmissionResultDto>.NotFoundResponse("Attempt not found.");

        if (attempt.AttemptStatus != AttemptStatus.InProgress)
            return EndpointResponse<SubmissionResultDto>.ErrorResponse(
                "This attempt has already been submitted.");

        var now = DateTime.UtcNow;

        // Deadline check — accept late submissions but mark as auto-submitted
        // (SaveAnswer would have already rejected post-deadline individual answers)
        bool isAutoSubmit = request.IsAutoSubmit || now > attempt.DeadlineAt;

        attempt.SubmittedAt     = now;
        attempt.IsAutoSubmitted = isAutoSubmit;
        attempt.TimeTakenSeconds = (int)(now - attempt.StartedAt).TotalSeconds;

        // ── Auto-grade SingleChoice and TrueFalse answers ────────────────────
        int autoGradedScore = 0;
        bool hasEssayQuestions = false;

        foreach (var answer in attempt.Answers)
        {
            if (answer.Question.Type == QuestionType.Essay)
            {
                hasEssayQuestions = true;
                // IsCorrect stays null permanently for essays (per design §3.3 amendment)
                continue;
            }

            if (answer.SelectedChoiceId.HasValue && answer.SelectedChoice is not null)
            {
                bool isCorrect    = answer.SelectedChoice.IsCorrect;
                answer.IsCorrect  = isCorrect;
                answer.PointsAwarded = isCorrect ? answer.Question.Points : 0;
                autoGradedScore  += answer.PointsAwarded.Value;
            }
            else
            {
                // Unanswered choice question
                answer.IsCorrect     = false;
                answer.PointsAwarded = 0;
            }
            answer.UpdatedAt = now;
        }

        attempt.AutoGradedScore = autoGradedScore;

        if (hasEssayQuestions)
        {
            // Hold in PendingGrading — teacher must grade essays
            attempt.AttemptStatus = AttemptStatus.PendingGrading;
        }
        else
        {
            // No essays — finalise immediately
            attempt.EssayScore    = 0;
            attempt.FinalScore    = autoGradedScore;
            attempt.IsPassed      = attempt.MaxScore > 0 &&
                                    ((decimal)autoGradedScore / attempt.MaxScore * 100m)
                                    >= attempt.Exam.PassThresholdPercent;
            attempt.AttemptStatus = AttemptStatus.Graded;
        }

        await _db.SaveChangesAsync(cancellationToken);

        string gradingStatus = hasEssayQuestions ? "PendingEssayGrading" : "Complete";

        return EndpointResponse<SubmissionResultDto>.SuccessResponse(
            new SubmissionResultDto(
                attempt.Id,
                attempt.AttemptStatus.ToString(),
                attempt.AutoGradedScore,
                attempt.MaxScore,
                gradingStatus,
                attempt.FinalScore,
                attempt.IsPassed,
                attempt.TimeTakenSeconds),
            hasEssayQuestions
                ? "Exam submitted. Essay questions are pending teacher review."
                : "Exam submitted and graded.");
    }
}
